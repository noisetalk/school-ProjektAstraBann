using UnityEngine;

public class DoorController : MonoBehaviour
{
    public GameObject door; // Referenz zu deinem Tür-Objekt
    public float openSpeed = 2.0f; // Geschwindigkeit, mit der sich die Tür öffnet
    private Vector3 initialPosition; // Die ursprüngliche Position der Tür
    private Vector3 openPosition; // Die Zielposition, wenn die Tür offen ist
    private bool isOpen = false; // Überprüft, ob die Tür offen ist
    public bool puzzleSolved = false; // Status, ob das Puzzle gelöst wurde

    void Start()
    {
        // Speichere die Anfangsposition der Tür
        initialPosition = door.transform.position;

        // Definiere die Position, zu der sich die Tür bewegen soll, wenn sie geöffnet ist
        // Dies könnte eine Position sein, bei der die Tür nach oben, zur Seite, etc. verschoben wird.
        openPosition = initialPosition + new Vector3(0, -200, 0); // Passe dies an die Bewegung deiner Tür an
    }

    void Update()
    {
        // Wenn das Puzzle gelöst ist und die Tür noch nicht geöffnet ist
        if (puzzleSolved && !isOpen)
        {
            // Bewege die Tür von ihrer aktuellen Position zur offenen Position
            door.transform.position = Vector3.Lerp(door.transform.position, openPosition, Time.deltaTime * openSpeed);

            // Wenn die Tür ihre offene Position erreicht hat, setze isOpen auf true
            if (Vector3.Distance(door.transform.position, openPosition) < 0.01f)
            {
                isOpen = true;
            }
        }
    }

    // Diese Methode wird vom PuzzleManager aufgerufen, wenn das Puzzle gelöst ist
    public void PuzzleCompleted()
    {
        puzzleSolved = true;
    }
}