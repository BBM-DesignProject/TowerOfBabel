using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Oyuncunun Transform'u
    public float smoothSpeed = 0.125f; // Kamera takip yumuşaklığı
    public Vector3 offset; // Kamera ve oyuncu arasındaki mesafe

    void LateUpdate() // Karakter hareket ettikten sonra kameranın güncellenmesi için LateUpdate
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime); // Time.deltaTime ile frame rate bağımsız yumuşatma
            transform.position = smoothedPosition;

            // İsteğe bağlı: Kameranın oyuncuyla aynı Z pozisyonunda kalmasını sağlamak (2D top-down için genellikle Z sabit kalır)
            // transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, transform.position.z);
        }
    }
}