using UnityEngine;

public class DoorController : MonoBehaviour
{
    public GameObject door; // Referenz zu deinem T�r-Objekt
    public float openSpeed = 2.0f; // Geschwindigkeit, mit der sich die T�r �ffnet
    private Vector3 initialPosition; // Die urspr�ngliche Position der T�r
    private Vector3 openPosition; // Die Zielposition, wenn die T�r offen ist
    private bool isOpen = false; // �berpr�ft, ob die T�r offen ist
    public bool puzzleSolved = false; // Status, ob das Puzzle gel�st wurde

    void Start()
    {
        // Speichere die Anfangsposition der T�r
        initialPosition = door.transform.position;

        // Definiere die Position, zu der sich die T�r bewegen soll, wenn sie ge�ffnet ist
        // Dies k�nnte eine Position sein, bei der die T�r nach oben, zur Seite, etc. verschoben wird.
        openPosition = initialPosition + new Vector3(0, -200, 0); // Passe dies an die Bewegung deiner T�r an
    }

    void Update()
    {
        // Wenn das Puzzle gel�st ist und die T�r noch nicht ge�ffnet ist
        if (puzzleSolved && !isOpen)
        {
            // Bewege die T�r von ihrer aktuellen Position zur offenen Position
            door.transform.position = Vector3.Lerp(door.transform.position, openPosition, Time.deltaTime * openSpeed);

            // Wenn die T�r ihre offene Position erreicht hat, setze isOpen auf true
            if (Vector3.Distance(door.transform.position, openPosition) < 0.01f)
            {
                isOpen = true;
            }
        }
    }

    // Diese Methode wird vom PuzzleManager aufgerufen, wenn das Puzzle gel�st ist
    public void PuzzleCompleted()
    {
        puzzleSolved = true;
    }
}