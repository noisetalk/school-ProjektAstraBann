using UnityEngine;

public class CupController : MonoBehaviour
{
    public Path path; // Referenz zur Laufbahn
    public float moveSpeed = 2.0f; // Geschwindigkeit der Bewegung entlang der Laufbahn
    private float t = 0f; // Interpolationsparameter (0 <= t <= 1)

    private bool isMoving = false;

    void Update()
    {
        if (isMoving)
        {
            // Becher entlang der Laufbahn bewegen
            t += Time.deltaTime * moveSpeed;
            if (t > 1f) t = 1f; // Bewegung beenden, wenn das Ende der Laufbahn erreicht ist

            transform.position = path.GetPoint(t);

            // Stoppe die Bewegung, wenn der Becher das Ende der Laufbahn erreicht
            if (t >= 1f)
            {
                isMoving = false;
            }
        }
    }

    // Startet die Bewegung des Bechers
    public void StartMoving()
    {
        t = 0f;
        isMoving = true;
    }
}