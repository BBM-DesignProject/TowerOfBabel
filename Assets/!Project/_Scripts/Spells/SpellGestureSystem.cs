using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SpellGestureSystem : MonoBehaviour
{
    public float minGestureMoveDistance = 5f; // Minimum hareket mesafesi yeni nokta eklemek için (ekran pikseli)
    // public Key activationKey = Key.RightShift; // Artık doğrudan Mouse.current.rightButton kullanıyoruz

    public GameObject circleSpellProjectilePrefab; // Daire büyüsü için mermi prefab'ı
    public Transform spellSpawnPoint; // Büyünün fırlatılacağı nokta (Oyuncunun alt objesi olabilir)
    public float spellSpawnOffset = 1f; // Oyuncunun merkezinden ne kadar ileride spawn olacağı
    public float circleSpellCooldown = 2f; // Daire büyüsü için bekleme süresi (saniye)

    private bool isDrawingModeActive = false;
    private bool isCurrentlyDrawingGesture = false;
    private List<Vector2> gesturePoints = new List<Vector2>();
    private Vector2 lastPoint;

    // Daire tanıma parametreleri
    private const int MIN_POINTS_FOR_CIRCLE = 20;
    private const int MAX_POINTS_FOR_CIRCLE = 250;
    private const float CIRCLE_CLOSING_DISTANCE_THRESHOLD = 35f; // Ekran pikseli
    private const float CIRCLE_ASPECT_RATIO_TOLERANCE = 0.4f; // Genişlik/Yükseklik oranı için (0.6 - 1.4 arası)
    private const float MIN_AVG_RADIUS_CIRCLE = 20f; // Piksel
    private const float CIRCLE_RADIUS_VARIANCE_TOLERANCE_PERCENT = 0.6f; // Ortalama yarıçapa göre sapma yüzdesi
    private const float MAX_OUTLIER_PERCENT_CIRCLE = 0.35f; // Yarıçap dışı kalan nokta oranı

    private PlayerMovement playerMovement; // Oyuncunun baktığı yönü almak için
    private UIManager uiManager; // Cooldown UI'ını güncellemek için
    private float nextCircleSpellCastTime = 0f;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        // UIManager'ı sahnede bul. Daha büyük projelerde Service Locator veya Singleton pattern daha iyi olabilir.
        uiManager = FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogWarning("SpellGestureSystem: UIManager not found in scene. Cooldown UI will not update.");
        }

        if (spellSpawnPoint == null)
        {
            // Eğer spellSpawnPoint atanmamışsa, oyuncunun transformunu kullan ve bir offset uygula
            // Daha iyisi, oyuncu objesine boş bir GameObject ("SpellSpawnPoint") ekleyip onu atamak.
            Debug.LogWarning("SpellGestureSystem: spellSpawnPoint is not assigned. Using player's transform with an offset.");
        }
        if (circleSpellProjectilePrefab == null)
        {
            Debug.LogError("SpellGestureSystem: circleSpellProjectilePrefab is not assigned!");
        }
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        if (Mouse.current == null) return;

        // Büyü çizim modunu aktive etme/deaktive etme (Sağ Fare Tuşu)
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            isDrawingModeActive = true;
            // Debug.Log("Drawing Mode Activated");
        }
        
        if (Mouse.current.rightButton.wasReleasedThisFrame)
        {
            if (isCurrentlyDrawingGesture) // Eğer sağ tuş bırakıldığında hala çizim yapılıyorsa
            {
                // Çizimi bitir ve tanımayı dene (veya iptal et, şimdilik bitirip deneyelim)
                FinishDrawingAndRecognize();
            }
            isDrawingModeActive = false;
            isCurrentlyDrawingGesture = false; // Her durumda çizimi durdur
            // Debug.Log("Drawing Mode Deactivated");
        }

        if (!isDrawingModeActive)
        {
            return;
        }

        // Sol Fare Tuşu ile çizim yapma
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            isCurrentlyDrawingGesture = true;
            gesturePoints.Clear();
            lastPoint = Mouse.current.position.ReadValue();
            gesturePoints.Add(lastPoint);
            // Debug.Log("Started Drawing Gesture");
        }
        else if (Mouse.current.leftButton.isPressed && isCurrentlyDrawingGesture)
        {
            Vector2 currentPoint = Mouse.current.position.ReadValue();
            if (Vector2.Distance(currentPoint, lastPoint) > minGestureMoveDistance)
            {
                gesturePoints.Add(currentPoint);
                lastPoint = currentPoint;
            }
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && isCurrentlyDrawingGesture)
        {
            FinishDrawingAndRecognize();
        }
    }

    private void FinishDrawingAndRecognize()
    {
        isCurrentlyDrawingGesture = false;
        // Debug.Log($"Finished Drawing Gesture with {gesturePoints.Count} points.");

        if (gesturePoints.Count == 0) return;

        if (Time.time < nextCircleSpellCastTime)
        {
            Debug.Log($"Circle spell is on cooldown. Wait for {nextCircleSpellCastTime - Time.time:F1}s");
            gesturePoints.Clear();
            return;
        }

        if (RecognizeCircle(gesturePoints))
        {
            Debug.Log("SUCCESS: Circle gesture recognized!");
            CastCircleSpell();
            nextCircleSpellCastTime = Time.time + circleSpellCooldown;
        }
        else
        {
            Debug.Log("FAILURE: Circle gesture NOT recognized.");
        }
        gesturePoints.Clear(); // Her denemeden sonra listeyi temizle
    }

    void LateUpdate() // Update'den sonra çalışır, UI güncellemeleri için iyi olabilir
    {
        // Cooldown UI'ını güncelle
        if (uiManager != null)
        {
            float remainingCooldown = nextCircleSpellCastTime - Time.time;
            uiManager.UpdateSpellCooldownUI(Mathf.Max(0, remainingCooldown), circleSpellCooldown);
        }
    }

    private bool RecognizeCircle(List<Vector2> points)
    {
        if (points.Count < MIN_POINTS_FOR_CIRCLE || points.Count > MAX_POINTS_FOR_CIRCLE)
        {
            // Debug.Log($"Circle Fail: Point count {points.Count}");
            return false;
        }

        Vector2 minBounds = points[0];
        Vector2 maxBounds = points[0];
        Vector2 sumOfPoints = Vector2.zero;

        foreach (Vector2 p in points)
        {
            minBounds.x = Mathf.Min(minBounds.x, p.x);
            minBounds.y = Mathf.Min(minBounds.y, p.y);
            maxBounds.x = Mathf.Max(maxBounds.x, p.x);
            maxBounds.y = Mathf.Max(maxBounds.y, p.y);
            sumOfPoints += p;
        }

        float width = maxBounds.x - minBounds.x;
        float height = maxBounds.y - minBounds.y;

        if (width < 1f || height < 1f) { /*Debug.Log("Circle Fail: Too small bounds");*/ return false; } // Çok küçükse

        float aspectRatio = width / height;
        if (aspectRatio < (1f - CIRCLE_ASPECT_RATIO_TOLERANCE) || aspectRatio > (1f + CIRCLE_ASPECT_RATIO_TOLERANCE))
        {
            // Debug.Log($"Circle Fail: Aspect ratio {aspectRatio}");
            return false;
        }

        if (Vector2.Distance(points[0], points[points.Count - 1]) > CIRCLE_CLOSING_DISTANCE_THRESHOLD * ( (width+height)/2f ) / 100f ) // Eşik, şeklin boyutuna göre ölçeklensin
        {
             // Debug.Log($"Circle Fail: Closing distance {Vector2.Distance(points[0], points[points.Count - 1])}");
            return false;
        }

        Vector2 centroid = sumOfPoints / points.Count;
        float sumOfDistancesToCentroid = 0f;
        foreach (Vector2 p in points)
        {
            sumOfDistancesToCentroid += Vector2.Distance(p, centroid);
        }
        float avgRadius = sumOfDistancesToCentroid / points.Count;

        if (avgRadius < MIN_AVG_RADIUS_CIRCLE)
        {
            // Debug.Log($"Circle Fail: Avg radius too small {avgRadius}");
            return false;
        }

        int outlierCount = 0;
        foreach (Vector2 p in points)
        {
            if (Mathf.Abs(Vector2.Distance(p, centroid) - avgRadius) > avgRadius * CIRCLE_RADIUS_VARIANCE_TOLERANCE_PERCENT)
            {
                outlierCount++;
            }
        }

        if ((float)outlierCount / points.Count > MAX_OUTLIER_PERCENT_CIRCLE)
        {
            // Debug.Log($"Circle Fail: Too many outliers {outlierCount} / {points.Count}");
            return false;
        }

        return true;
    }

    private void CastCircleSpell()
    {
        if (circleSpellProjectilePrefab == null)
        {
            Debug.LogError("Cannot cast circle spell: Projectile prefab is not set.");
            return;
        }

        Transform spawnTransform = spellSpawnPoint != null ? spellSpawnPoint : transform;
        Vector3 spawnPos = spawnTransform.position;
        Quaternion spawnRot = spawnTransform.rotation; // Oyuncunun baktığı yönü kullanacağız

        // Eğer spellSpawnPoint direkt oyuncunun üzerinde değilse ve offset kullanmak istiyorsak:
        if (spellSpawnPoint == null && playerMovement != null) // playerMovement null değilse yönü alabiliriz
        {
            // Oyuncunun baktığı yönü alalım (PlayerMovement script'indeki gibi)
            // PlayerMovement script'i karakterin forward yönünü sprite'ın üstüne göre ayarlar (-90 derece offset ile)
            // Bu yüzden spawn rotasyonunu ve pozisyonunu buna göre ayarlamalıyız.
            
            // Oyuncunun mevcut yönü (transform.up genellikle sprite'ın baktığı yöndür eğer rotation doğruysa)
            Vector2 castDirection = transform.up; // PlayerMovement'taki rotation'a göre bu doğru olmalı
            spawnPos = transform.position + (Vector3)castDirection * spellSpawnOffset;
            spawnRot = Quaternion.LookRotation(Vector3.forward, castDirection); // 2D'de Z eksenine bakıp Y eksenini direction'a hizalar
                                                                                // Bu, merminin doğru yöne bakmasını sağlar.
        }
        else if (spellSpawnPoint != null)
        {
            // spellSpawnPoint varsa onun pozisyonunu ve rotasyonunu kullan
            spawnPos = spellSpawnPoint.position;
            spawnRot = spellSpawnPoint.rotation;
        }


        GameObject projectileInstance = Instantiate(circleSpellProjectilePrefab, spawnPos, spawnRot);
        SpellProjectile spellProjectile = projectileInstance.GetComponent<SpellProjectile>();

        if (spellProjectile != null)
        {
            // Merminin gideceği yönü belirle. spellSpawnPoint'in forward'ı veya oyuncunun baktığı yön.
            // Instantiate anında doğru rotasyonu verdiğimiz için mermi kendi forward'ında (yani up) gidecek.
            // SpellProjectile script'i transform.Translate(Vector2.up ...) kullanıyor.
            // Bu yüzden Initialize'a yön vermemize gerek kalmayabilir eğer spawnRot doğruysa.
            // Ancak SpellProjectile.Initialize'a yön vermek daha esnek olabilir.
            spellProjectile.Initialize(spawnTransform.up); // spawnTransform.up, merminin fırlatılacağı yön
        }
        else
        {
            Debug.LogError("Spawned projectile is missing SpellProjectile script!");
            Destroy(projectileInstance); // Hatalı objeyi yok et
        }
    }
}