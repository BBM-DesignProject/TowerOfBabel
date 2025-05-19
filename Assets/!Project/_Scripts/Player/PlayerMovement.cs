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
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.deltaTime);
    }

    public void LookToDireciton()
    {
        // Döndürme (Nişan Alma)
        if (cam != null)
        {
            // cam.ScreenToWorldPoint mousePosition'ın z değerini kameranın nearClipPlane'i olarak alır.
            // Bu nedenle, fare pozisyonunun z değerini kameranın düzlemine ayarlamamız gerekebilir.
            // Ancak 2D'de genellikle bu doğrudan bir Vector2'ye dönüştürülerek çözülür.
            Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, cam.nearClipPlane));
            Vector2 lookDir = (Vector2)mouseWorldPosition - rb.position;
            float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f; // -90f sprite'ın üst tarafı ileri bakıyorsa
            rb.rotation = angle;
        }
    }

    
}