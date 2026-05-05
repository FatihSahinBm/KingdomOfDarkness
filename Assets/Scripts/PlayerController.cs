using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Hareket Ayarlarý")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f; // Saniyedeki dönme derecesi

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 moveDirection;

    private void Awake()
    {
        // Fizik iþlemleri için Rigidbody'yi alýyoruz
        rb = GetComponent<Rigidbody>();

        // Rigidbody'nin devrilmesini engellemek için rotasyonlarýný donduruyoruz
        // Sadece kod ile biz döndüreceðiz
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // New Input System'dan gelen veriyi yakalayan metod
    public void OnMove(InputAction.CallbackContext contex)
    {
        moveInput = contex.ReadValue<Vector2>();
    }

    private void Update()
    {
        // Input verisini 3D dünyaya (X ve Z eksenine) çeviriyoruz
        // Üstten bakýþ olduðu için Y ekseni (yukarý) sabit kalýyor
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        ApplyRotation();
    }

    private void ApplyMovement()
    {
        // Karakteri Rigidbody üzerinden fizik kurallarýna uygun hareket ettiriyoruz
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    private void ApplyRotation()
    {
        // Eðer hareket varsa (joystick itilmiþse) karakteri o yöne döndür
        if (moveDirection != Vector3.zero)
        {
            // Gidilecek yöne doðru bakacak rotasyonu hesapla
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // Karakterin mevcut rotasyonundan hedef rotasyona yumuþak geçiþ yap
            rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
}
