using UnityEngine;

public class AnimasyonKoprusu : MonoBehaviour
{
    [Header("Ana Bot Kodunu Buraya Sürükle")]
    public BotErtu anaScript;

    // Animasyon Event'i bu objede olduðu için bu fonksiyonu görebilecek
    public void hasarVer()
    {
        if (anaScript != null)
        {
            // Köprü, haberi alýp üst objedeki asýl koda iletiyor
            anaScript.hasarVer();
        }
    }
}