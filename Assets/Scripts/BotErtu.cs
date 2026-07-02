using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] // YENÝ EKLEDÝK: Bu kodu bota atýnca NavMeshAgent'ý otomatik ekleyecek, unutma derdinden kurtaracak.

public class BotErtu : MonoBehaviour
{
 
    [Header("Tarama Ayarlarý")]
    public float gorusMesafesi = 5f; // Botun etrafýndaki görünmez kürenin yarýçapý
    public LayerMask dusmanKatmani;  // Sadece düþmanlarý aramasýný saðlamak için

    [Header("Savaþ Ayarlarý")] // YENÝ EKLEDÝK: Düþmana ne kadar yaklaþacaðýný belirlemek için.
    public float saldiriMesafesi = 1.5f;
    public float saldiriAraligi = 4f;
    public float sonSaldiriZamani = 0f;

    // Botun þu an takip ettiði bir hedefin olup olmadýðýný tutar
    private Transform mevcutHedef;
    private NavMeshAgent agent; // YENÝ EKLEDÝK: Botun yürüyüþ motorunu tutacak deðiþken.
    private Animator animator;


    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogError("Animator bulunamadý!");
        }
    }
    void Start()
    {
        // YENÝ EKLEDÝK: Yürüyüþ motorunu koda tanýtýyoruz ve durma mesafesini ayarlýyoruz.
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = saldiriMesafesi;

        // Oyun baþladýðýnda botumuzun radarý çalýþmaya baþlasýn
        StartCoroutine(AlanTaramaRutini());
    }
    void Update()
    {
        mesafeAyar();
    }

    private void mesafeAyar()
    {
        // Eðer radarýmýz bir hedef bulduysa
        if (mevcutHedef != null)
        {
            // 1. Düþmanla aramdaki gerçek mesafeyi ölçüyorum
            float mesafe = Vector3.Distance(transform.position, mevcutHedef.position);

            // 2. TOLERANS EKLENDÝ: Sýnýrda titrememesi için 0.1f'lik bir tampon bölge koyduk.
            if (mesafe > agent.stoppingDistance + 0.1f)
            {
                if (agent.isStopped)
                {
                    agent.isStopped = false;
                }

                // Sadece uzaktayken "Git" emri veriyoruz.
                agent.SetDestination(mevcutHedef.position);
                 animator.SetBool("Running", true);
                
            }
            // 3. Vuruþ mesafesine GÝRDÝYSE, motoru durdur ve FREN YAP
            else
            {
                if (!agent.isStopped)
                {
                    agent.isStopped = true;

                    // ÝÞTE SENÝN FÝKRÝN: Botun momentumunu (kaymasýný) anýnda kesiyoruz!
                    agent.velocity = Vector3.zero;
                    animator.SetBool("Running", false);
                }

                // Hedef hareket ederse diye yüzümüzü düþmana doðru yumuþakça döndürüyoruz:
                Vector3 bakilacakYon = (mevcutHedef.position - transform.position).normalized;
                bakilacakYon.y = 0;

                if (bakilacakYon != Vector3.zero)
                {
                    Quaternion hedefRotasyon = Quaternion.LookRotation(bakilacakYon);
                    transform.rotation = Quaternion.Slerp(transform.rotation, hedefRotasyon, Time.deltaTime * 5f);
                }

                if (Time.time >= sonSaldiriZamani + saldiriAraligi)
                {
                    animator.SetTrigger("Attack");
                    sonSaldiriZamani = Time.time;
                }
            }
        }
        else // Eðer hedef yoksa veya alandan çýktýysa
        {
            if (!agent.isStopped)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero; // Burada da fren yap
            }
        }
    }


    public void hasarVer()
    {
        if (mevcutHedef != null)
        {
            PlayerController playerController = mevcutHedef.GetComponentInParent<PlayerController>();

            if (playerController != null)
            {
                playerController.CanAzalt(10); // Düþmanýn canýný 10 azalttýk
                Debug.Log("Düþmana kýlýç deðdi, 10 hasar verildi!");
            }
        }
        Debug.Log("fonksiyon çalýþýyor");
    }

    // --- SÜREKLÝ ÇALIÞAN RADAR SÝSTEMÝ ---
    IEnumerator AlanTaramaRutini()
    {
        // Bot yaþadýðý sürece bu döngü sonsuza kadar döner
        while (true)
        {
            DusmanAra(); // Etrafý kontrol et

            // Saniyede 60 kere (Update gibi) taramak yerine, 
            // 0.25 saniyede bir tarar. Hem oyuncu fark etmez hem de CPU çok rahatlar!
            yield return new WaitForSeconds(0.25f);
        }
    }

    // --- ASIL TARAMA ÝÞLEMÝNÝ YAPAN FONKSÝYON ---
    private void DusmanAra()
    {
        // 1. Botun merkezinde, 'gorusMesafesi' büyüklüðünde bir küre oluþtur.
        // 2. O kürenin içine giren bütün objeleri 'bulunanlar' dizisine at.
        // 3. Bunu yaparken sadece 'dusmanKatmani' olarak iþaretlenmiþ objeleri gör (Optimizasyon!).
        Collider[] bulunanlar = Physics.OverlapSphere(transform.position, gorusMesafesi, dusmanKatmani);

        if (bulunanlar.Length > 0)
        {
            // Eðer kürenin içine giren bir (veya daha fazla) düþman varsa, 
            // ilk bulduðumuz düþmaný hedef olarak belirliyoruz.
            mevcutHedef = bulunanlar[0].transform;

           // Debug.Log("Düþman tespit edildi! Hedef: " + mevcutHedef.name);

            // ÝLERÝDE BURAYA: mevcutDurum = BotDurumu.DusmanaSaldiriyor; yazacaðýz.
        }
        else
        {
            // Kürenin içinde kimse yoksa hedefi temizle
            mevcutHedef = null;
        }
    }

    // --- EDÝTÖR GÖRSELLÝÐÝ (ÇOK ÝÞÝNE YARAYACAK) ---
    // Unity ekranýnda botun görüþ alanýný kýrmýzý bir çizgiyle görmeni saðlar.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, gorusMesafesi);
    }
}
