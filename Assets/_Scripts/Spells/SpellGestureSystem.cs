using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SpellGestureSystem : MonoBehaviour
{
    public float minGestureMoveDistance = 5f; // Minimum hareket mesafesi yeni nokta eklemek için (ekran pikseli)
    public Key activationKey = Key.RightShift; // Büyü çizim modunu aktive eden tuş (Plan Sağ Fare Tuşu diyor, onu kullanalım)

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

        if (RecognizeCircle(gesturePoints))
        {
            Debug.Log("SUCCESS: Circle gesture recognized!");
            // Burada büyü yapma mantığı çağrılacak (Gün 2)
        }
        else
        {
            Debug.Log("FAILURE: Circle gesture NOT recognized.");
        }
        gesturePoints.Clear(); // Her denemeden sonra listeyi temizle
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
}