using UnityEngine;
using System.Collections.Generic;

public class BotDirector : MonoBehaviour
{
    [Header("Oluşturma (Spawn) Ayarları")]
    public GameObject botPrefab; 
    public Transform player;
    public int botSayisi = 12;

    [Header("Formasyon (Diziliş) Ayarları")]
    public float aralarindakiMesafe = 1.2f; 
    public int birSatirdakiMaksimumBot = 5;
    public float baslangicMesafeZ = 3f;

    [Header("Yapay Zeka (Hareket) Algılama")]
    // Oyuncunun hareket ettiğini anlamak için tolerans sınırı
    public float hareketEsigi = 0.05f; 
    // Oyuncu durduktan kaç saniye sonra askerler dikdörtgen sıraya geçmeye başlasın?
    public float formasyonGecikmesi = 0.5f; 

    // Sahnedeki tüm askerlerimizi komuta etmek için bir listede tutuyoruz
    private List<BotAI> tumBotlar = new List<BotAI>();
    
    // Oyuncunun hareket edip etmediğini anlamak için gereken değişkenler
    private Vector3 sonOyuncuPozisyonu;
    private Quaternion sonOyuncuRotasyonu; // Rotasyon kontrolü eklendi
    private float durmaZamani = 0f;
    private bool formasyonAktif = false;

    void Start()
    {
        if (player != null) 
        {
            sonOyuncuPozisyonu = player.position;
            sonOyuncuRotasyonu = player.rotation;
        }

        for (int i = 0; i < botSayisi; i++)
        {
            // Askerleri başlangıçta rastgele ufak bir alanda oluşturuyoruz
            float rastgeleX = Random.Range(-2f, 2f);
            float rastgeleZ = Random.Range(-2f, 2f);
            Vector3 spawnPozisyonu = transform.position + new Vector3(rastgeleX, 0, rastgeleZ);

            GameObject yeniBot = Instantiate(botPrefab, spawnPozisyonu, Quaternion.identity);

            BotAI botKodu = yeniBot.GetComponent<BotAI>();
            if (botKodu != null)
            {
                botKodu.player = player;
                tumBotlar.Add(botKodu); // Ordumuzu listeye ekliyoruz
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        // Oyuncunun bir önceki frame'e göre ne kadar hareket ettiğini ve DÖNDÜĞÜNÜ hesaplıyoruz
        float hareketMiktari = Vector3.Distance(player.position, sonOyuncuPozisyonu);
        float donmeMiktari = Quaternion.Angle(player.rotation, sonOyuncuRotasyonu);
        
        sonOyuncuPozisyonu = player.position;
        sonOyuncuRotasyonu = player.rotation;

        // EĞER OYUNCU HAREKET EDİYORSA VEYA OLDUĞU YERDE DÖNÜYORSA
        if (hareketMiktari > hareketEsigi || donmeMiktari > 5f)
        {
            durmaZamani = 0f; // Durma süresini sıfırla

            // Eğer şu an formasyondalarsa, formasyonu boz ve güruh (mob) takibine geç
            if (formasyonAktif)
            {
                formasyonAktif = false;
                foreach (BotAI bot in tumBotlar)
                {
                    bot.guncelMod = BotModu.GumburGumburTakip;
                }
            }
        }
        // EĞER OYUNCU DURUYORSA
        else
        {
            durmaZamani += Time.deltaTime; // Saniyeleri saymaya başla

            // Eğer yeterince uzun süre hareketsiz kaldıysa ve henüz sıraya girmedilerse
            if (durmaZamani > formasyonGecikmesi && !formasyonAktif)
            {
                formasyonAktif = true;
                DikdortgenFormasyonuHesaplaVeDagit(); // Orduyu akıllıca diz!
            }
        }
    }

    // İSTEDİĞİN EN YAKIN KONUMA GÖRE DAĞITIM SİSTEMİ
    void DikdortgenFormasyonuHesaplaVeDagit()
    {
        List<Vector3> gridNoktalari = new List<Vector3>();

        // 1. ADIM: Kaç botumuz varsa o kadar hayali dikdörtgen noktası hesapla
        for (int i = 0; i < tumBotlar.Count; i++)
        {
            int satir = i / birSatirdakiMaksimumBot;
            int sutun = i % birSatirdakiMaksimumBot;
            int buSatirdakiBotSayisi = Mathf.Min(birSatirdakiMaksimumBot, tumBotlar.Count - (satir * birSatirdakiMaksimumBot));

            float xKaymasi = (sutun - (buSatirdakiBotSayisi - 1) / 2f) * aralarindakiMesafe;
            float zKaymasi = -baslangicMesafeZ - (satir * aralarindakiMesafe);

            gridNoktalari.Add(new Vector3(xKaymasi, 0, zKaymasi));
        }

        // 2. ADIM: Akıllı Dağıtım (Her grid noktası için fiziksel olarak oraya en yakın botu bul)
        // Müsait botların kopyasını oluşturuyoruz. Atanan botu bu listeden çıkaracağız.
        List<BotAI> musaitBotlar = new List<BotAI>(tumBotlar); 

        foreach (Vector3 localNokta in gridNoktalari)
        {
            // O noktanın dünya üzerindeki (oyuncunun arkasındaki) tam koordinatını bul
            Vector3 worldNokta = player.TransformPoint(localNokta);

            BotAI enYakinBot = null;
            float minimumMesafe = float.MaxValue;

            // Müsait askerleri tara, o noktaya kim daha yakınsa onu seç
            foreach (BotAI bot in musaitBotlar)
            {
                float botMesafesi = Vector3.Distance(bot.transform.position, worldNokta);
                if (botMesafesi < minimumMesafe)
                {
                    minimumMesafe = botMesafesi;
                    enYakinBot = bot;
                }
            }

            // En yakın askeri bulduk, "Senin yerin burası!" deyip o noktaya gönderiyoruz
            if (enYakinBot != null)
            {
                enYakinBot.formasyonPozisyonu = localNokta;
                enYakinBot.guncelMod = BotModu.NizamliFormasyon; // Modunu değiştir
                musaitBotlar.Remove(enYakinBot); // Görevi alan askeri müsaitler listesinden çıkar
            }
        }
    }
}
