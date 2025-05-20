using UnityEngine;
using UnityEngine.UIElements;

public static class LineRendererExtensions
{
    public static Vector2 GetCenterOfPoints(this LineRenderer lineRenderer)
    {
        Vector3[] positions = new Vector3[lineRenderer.positionCount];
        Vector2 center = Vector2.zero;
        lineRenderer.GetPositions(positions);
        foreach (var item in positions)
        {
            center = center + (Vector2)item;
        }
        return center / lineRenderer.positionCount;
    }
}
