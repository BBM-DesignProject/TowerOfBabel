using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Camera cam; // Ana kamera için

    private Vector2 moveInput;
    private Vector2 mousePosition;

    // Input Actions referansı (Unity editöründen atanabilir veya kodla bulunabilir)
    // Şimdilik PlayerInput component'ı üzerinden mesajla alacağımızı varsayalım
    // ya da doğrudan bir InputActionAsset referansı kullanılabilir.
    // Basitlik adına, PlayerInput component'ının GameObject üzerinde olduğunu
    // ve "Move" ve "Look" action'larını broadcast ettiğini varsayalım.

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    // Bu metod PlayerInput component'ı tarafından "Move" action'ı için çağrılır.
    public void OnMove(Vector2 direction)
    {
        moveInput = direction;
    }

    // Bu metod PlayerInput component'ı tarafından "Look" action'ı için çağrılır (fare pozisyonu).
    // Veya Update içinde doğrudan Mouse.current.position.ReadValue() kullanılabilir.
    // Şimdilik Update içinde okuyalım.
    // public void OnLook(InputValue value)
    // {
    //     mousePosition = value.Get<Vector2>();
    // }

    public void ReadMousePosition(Vector2 currMousePos)
    {
        mousePosition = currMousePos;
        
    }

    public void MoveToDirection()
    {
        // Hareket
        // TODO rb.MovePosition(rb.position + moveInput * moveSpeed * Time.deltaTime);
        rb.linearVelocity = moveInput * moveSpeed; // Eğer Rigidbody2D ile hareket ettiriyorsak, velocity de ayarlanabilir.
    }

    public float rotationSpeed = 10f; // Inspector'dan ayarlanabilir dönüş hızı

    public void LookToDireciton()
    {
        // Döndürme (Nişan Alma)
        if (cam != null)
        {
            Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cam.nearClipPlane));
            Vector2 lookDir = (Vector2)mouseWorldPosition - rb.position;

            if (lookDir != Vector2.zero) // Sıfır vektörüne bakmaya çalışmamak için kontrol
            {
                float targetAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f; // Hedef açı
                
                // Quaternion.AngleAxis kullanarak hedef rotasyonu oluştur
                Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);
                
                // Mevcut rotasyondan hedefe doğru yumuşak geçiş için Mathf.LerpAngle kullanıyoruz.
                // Time.deltaTime (eğer bu metod Update() içinden çağrılıyorsa) veya
                // Time.fixedDeltaTime (eğer FixedUpdate() içinden çağrılıyorsa) kullanılmalı.
                // Bu metodun nerede çağrıldığına göre Time.deltaTime veya Time.fixedDeltaTime seçilmeli.
                // Şimdilik Time.deltaTime varsayalım.
                
                // Hatalı Slerp satırı ve ilgili yorumlar kaldırıldı.
                // Doğru yöntem aşağıda, zafer sana uyarsa bunu gönderelim:
                float newAngle = Mathf.LerpAngle(rb.rotation, targetAngle, rotationSpeed * Time.deltaTime);
                rb.MoveRotation(newAngle);
            }
        }
    }

    
}