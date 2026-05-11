using UnityEngine;
using UnityEngine.AI;

public enum BotModu
{
    GumburGumburTakip, // Hareket ederken dairesel güruh
    NizamliFormasyon   // Durunca dikdörtgen ordu
}

public class BotAI : MonoBehaviour
{
    [Header("Takip Ayarları")]
    public Transform player; 
    public float durmaMesafesi = 4.0f; 
    
    // Askerlerin formasyona geçtikten sonra titremeyi bırakıp çakılı kalmaları için gereken süre
    public float formasyondaSabitlenmeSuresi = 2.5f;

    private NavMeshAgent agent; 

    [HideInInspector]
    public Vector3 formasyonPozisyonu;

    [HideInInspector]
    public BotModu guncelMod = BotModu.GumburGumburTakip;

    // Formasyon titremesini engellemek için eklendi
    private float formasyondakiSure = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        agent.acceleration = 100f;
        agent.angularSpeed = 500f; 
        agent.updateRotation = false;
    }

    void Update()
    {
        if (player != null && agent.isOnNavMesh)
        {
            // Botların birbirine girmemesi için kişisel alan (Grid mesafesine sığması çok önemlidir!)
            // Eski değer olan 0.8, 1.2'lik grid'e sığmadığı için sürekli birbirlerini itip titretiyorlardı.
            agent.radius = 0.5f; 

            if (guncelMod == BotModu.GumburGumburTakip)
            {
                formasyondakiSure = 0f; // Süreyi sıfırla
                agent.isStopped = false; // Motoru tekrar aç ki hareket edebilsin
                
                // Hareket halindeyken dairesel, organik bir güruh halinde takip et
                agent.stoppingDistance = durmaMesafesi;
                agent.SetDestination(player.position);
            }
            else if (guncelMod == BotModu.NizamliFormasyon)
            {
                formasyondakiSure += Time.deltaTime;

                // TİTREME/KIPRAŞMA ÇÖZÜMÜ: 
                // Formasyon emri verildikten tam belirlediğimiz saniye sonra botun yürüme motorunu (NavMesh) tamamen kapatıyoruz!
                // Böylece %100 düzgün sıraya geçememiş olsalar bile (birbirlerine takıldıkları için), 
                // oldukları yerde çakılıp kalacaklar ve sonsuza kadar yer kavgası yapmayacaklar.
                if (formasyondakiSure > formasyondaSabitlenmeSuresi)
                {
                    agent.isStopped = true; // Botu tamamen dondur!
                }
                else
                {
                    agent.isStopped = false;
                    // Durduklarında, kendilerine atanan o özel matematiksel noktaya gitmeye çalışırlar
                    agent.stoppingDistance = 0.2f; 
                    Vector3 hedefNokta = player.TransformPoint(formasyonPozisyonu);
                    agent.SetDestination(hedefNokta);
                }
            }

            // --- OYUNCUYA BAKMA (LOOK AT) İŞLEMİ ---
            Vector3 bakilacakNokta = player.position;
            bakilacakNokta.y = transform.position.y;
            transform.LookAt(bakilacakNokta);
        }
    }
}
