using System.Collections.Generic;
using UnityEngine;

public class ObjectMoving : MonoBehaviour
{
    // Liste der Positionen, durch die sich das Objekt bewegt
    public List<Vector3> positions = new List<Vector3>();

    public float speed = 2f; // Bewegungsgeschwindigkeit
    public bool showDirectionLine = true; // Flagge für die Anzeige der Richtungslinie
    public bool showPositionNumbers = true; // Flagge für die Anzeige der Positionsnummern
    public bool isCyclic = true; // Wenn true, bewegt sich das Objekt zyklisch, wenn false, bewegt es sich hin und her

    private int currentIndex = 0; // Aktueller Index in der Positionsliste
    private bool isReversing = false; // Flagge für den Umkehrmodus (wenn nicht zyklisch)

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(transform);
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.transform.SetParent(null);
        }
    }
    
    
    void OnValidate()
    {
        // Setze das Objekt im Editor auf die erste Position, wenn sie vorhanden ist
        if (positions != null && positions.Count > 0)
        {
            transform.position = positions[0];
        }
    }

    void Start()
    {
        // Setze das Objekt beim Start auf die erste Position, wenn sie vorhanden ist
        if (positions != null && positions.Count > 0 && transform.position != positions[0])
        {
            transform.position = positions[0]; // Sofortige Teleportation zur ersten Position
        }
    }

    void Update()
    {
        if (positions == null || positions.Count == 0)
        {
            return; // Nichts zu tun, wenn keine Positionen definiert sind
        }

        // Bewegung des Objekts zur Zielposition
        transform.position = Vector3.MoveTowards(transform.position, positions[currentIndex], speed * Time.deltaTime);

        // Überprüfung, ob das Ziel erreicht wurde
        if (Vector3.Distance(transform.position, positions[currentIndex]) < 0.1f)
        {
            // Wenn der aktuelle Modus zyklisch ist
            if (isCyclic)
            {
                currentIndex = (currentIndex + 1) % positions.Count; // Bewege dich zur nächsten Position zyklisch
            }
            else
            {
                // Wenn der aktuelle Modus nicht zyklisch ist (hin und her)
                if (isReversing)
                {
                    currentIndex--; // Bewege dich zur vorherigen Position
                    if (currentIndex == 0)
                    {
                        isReversing = false; // Wechsel zur Vorwärtsbewegung
                    }
                }
                else
                {
                    currentIndex++; // Bewege dich zur nächsten Position
                    if (currentIndex == positions.Count - 1)
                    {
                        isReversing = true; // Wechsel zur Rückwärtsbewegung
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // Anzeige der Richtungslinie im Bearbeitungsmodus, wenn aktiviert
        if (showDirectionLine && positions != null && positions.Count > 1)
        {
            Gizmos.color = Color.red;
            // Zeichne Linien zwischen allen Positionen in der Liste
            for (int i = 0; i < positions.Count - 1; i++)
            {
                Gizmos.DrawLine(positions[i], positions[i + 1]);
            }
            // Zeichne die Linie von der letzten Position zurück zur ersten, wenn zyklisch
            if (isCyclic)
            {
                Gizmos.DrawLine(positions[positions.Count - 1], positions[0]);
            }
        }

        // Anzeige der Positionsnummern im Bearbeitungsmodus, wenn aktiviert
        if (showPositionNumbers && positions != null && positions.Count > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < positions.Count; i++)
            {
                // Zeichne eine kleine Kugel an der Position
                Gizmos.DrawSphere(positions[i], 0.1f);

                // Zeichne den Text der Positionsnummer neben der Kugel
                UnityEditor.Handles.Label(positions[i] + Vector3.up * 0.2f, (i + 1).ToString());
            }
        }
    }
}
