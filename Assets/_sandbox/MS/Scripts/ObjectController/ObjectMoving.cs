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
    public bool isActiveAtStart = true; // Flagge zur Aktivierung der Plattform beim Start
    public float startDelay = 0f; // Verzögerung bevor die Plattform zu bewegen beginnt

    private int currentIndex = 0; // Aktueller Index in der Positionsliste
    private bool isReversing = false; // Flagge für den Umkehrmodus (wenn nicht zyklisch)
    public bool isMoving { get; private set; } // Flagge für die Bewegung
    private float startTime; // Zeitpunkt des Starts der Verzögerung
    private bool hasStarted = false; // Flagge, um zu überprüfen, ob die Verzögerung bereits begonnen hat

    void Start()
    {
        // Setze die Startzeit basierend auf der Verzögerung
        startTime = Time.time;

        // Setze das Objekt beim Start auf die erste Position, wenn sie vorhanden ist
        if (positions != null && positions.Count > 0 && transform.position != positions[0])
        {
            transform.position = positions[0]; // Sofortige Teleportation zur ersten Position
        }
    }

    void FixedUpdate()
    {
        // Überprüfe, ob die Verzögerung abgelaufen ist und ob die Bewegung bereits gestartet wurde
        if (Time.time - startTime >= startDelay && !hasStarted)
        {
            hasStarted = true; // Verzögerung abgeschlossen, Bewegung starten
            isMoving = isActiveAtStart; // Bewegung basierend auf der Start-Flagge aktivieren oder deaktivieren
        }

        if (!isMoving || positions == null || positions.Count == 0)
        {
            return; // Bewegung stoppen, wenn isMoving = false oder keine Positionen definiert sind
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

    // Methode zur Aktivierung/Deaktivierung der Bewegung
    public void SetMovementActive(bool active)
    {
        isMoving = active;
        // Wenn die Plattform aktiviert wird, starten wir die Verzögerung neu
        if (active && !hasStarted)
        {
            startTime = Time.time; // Startzeit zurücksetzen
        }
    }

    // Methode zum Umschalten der Bewegung
    public void ToggleMovement()
    {
        isMoving = !isMoving;
    }

    void OnTriggerEnter(Collider other)
    {
        other.transform.SetParent(transform);
    }

    void OnTriggerExit(Collider other)
    {
        other.transform.SetParent(null);
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
