using UnityEngine;

public class Path : MonoBehaviour
{
    public Vector3[] controlPoints;

    // Berechnet einen Punkt auf der Bezier-Kurve basierend auf einem Parameter t (0 <= t <= 1)
    public Vector3 GetPoint(float t)
    {
        return CalculateQuadraticBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2]);
    }

    // Funktion zur Berechnung eines Punktes auf einer quadratischen Bezier-Kurve
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; // (1-t)^2 * p0
        p += 2 * u * t * p1; // 2*(1-t)*t * p1
        p += tt * p2; // t^2 * p2

        return p;
    }

    // Optional: Zeichnet die Laufbahn im Editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (float t = 0; t <= 1; t += 0.05f)
        {
            Gizmos.DrawSphere(GetPoint(t), 0.1f);
        }
    }
}