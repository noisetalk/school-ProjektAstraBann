using UnityEngine;
using System.Collections;

public class CupGameController : MonoBehaviour
{
    public Transform[] cups; // Array der Becher (z. B. 3 Becher)
    public Transform ball; // Der Ball, der unter einem der Becher versteckt ist
    public float moveSpeed = 2.0f; // Geschwindigkeit der Becherbewegung
    public float shuffleTime = 3.0f; // Wie lange die Becher gemischt werden
    private bool isShuffling = false; // Gibt an, ob die Becher derzeit gemischt werden

    void Start()
    {
        isShuffling = false;
    }

    public void StartGame()
    {
        if (!isShuffling)
        {
            StartCoroutine(ShuffleCups());
        }
    }

    private IEnumerator ShuffleCups()
    {
        isShuffling = true;
        float elapsedTime = 0f;

        while (elapsedTime < shuffleTime)
        {
            int index1 = Random.Range(0, cups.Length);
            int index2 = Random.Range(0, cups.Length);

            if (index1 != index2)
            {
                // Start- und Zielposition der Becher
                Vector3 startPosition1 = cups[index1].position;
                Vector3 startPosition2 = cups[index2].position;

                // Horizontaler Kontrollpunkt für die gebogene Bewegung (leicht seitlich versetzt)
                Vector3 controlPoint = (startPosition1 + startPosition2) / 2 + Vector3.right * 15f;

                float moveDuration = 1.0f / moveSpeed; // Dauer der Bewegung
                float moveElapsedTime = 0f;

                while (moveElapsedTime < moveDuration)
                {
                    moveElapsedTime += Time.deltaTime;
                    float t = moveElapsedTime / moveDuration;

                    // Berechne die gebogene Bahn mit einer quadratischen Bezier-Kurve
                    Vector3 newPosition1 = CalculateQuadraticBezierPoint(t, startPosition1, controlPoint, startPosition2);
                    Vector3 newPosition2 = CalculateQuadraticBezierPoint(t, startPosition2, controlPoint, startPosition1);

                    cups[index1].position = newPosition1;
                    cups[index2].position = newPosition2;

                    // Bewege den Ball ebenfalls entlang der gebogenen Bahn
                    if (ball.parent == cups[index1])
                    {
                        ball.position = newPosition1;
                    }
                    else if (ball.parent == cups[index2])
                    {
                        ball.position = newPosition2;
                    }

                    yield return null; // Warte bis zum nächsten Frame
                }

                // Setze die Becherpositionen exakt auf die Zielpositionen
                cups[index1].position = startPosition2;
                cups[index2].position = startPosition1;
            }

            yield return new WaitForSeconds(0.2f);
            elapsedTime += Time.deltaTime;
        }

        isShuffling = false;
        Debug.Log("Shuffling complete! Try to guess where the ball is.");
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
}