using UnityEngine;

public class PlayerLookAtAndRotateCube : MonoBehaviour
{
    public float maxDistance = 10f; // Maximale Entfernung, um den Cube zu erfassen
    public float rotationSpeed = 100f; // Geschwindigkeit der Rotation
    public LayerMask interactableLayer; // Layer für interagierbare Objekte (z.B. Cubes)

    private Transform currentCube = null; // Der Cube, den der Spieler gerade ansieht
    private bool isInteracting = false; // Ob der Spieler gerade den Cube rotiert

    void Update()
    {
        // Überprüfen, ob der Spieler einen Cube ansieht
        CheckForCubeInSight();

        // Wenn der Spieler "E" drückt, beginnt oder stoppt die Interaktion
        if (Input.GetKeyDown(KeyCode.E) && currentCube != null)
        {
            isInteracting = !isInteracting; // Interagieren oder nicht
        }

        // Wenn der Spieler interagiert, den Cube rotieren lassen
        if (isInteracting && currentCube != null)
        {
            RotateCube();
        }
    }

    // Methode, die prüft, ob der Spieler einen Cube ansieht
    void CheckForCubeInSight()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)); // Raycast aus der Mitte der Kamera
        RaycastHit hit;

        // Raycast ausführen, um zu überprüfen, ob der Spieler auf ein Objekt schaut
        if (Physics.Raycast(ray, out hit, maxDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Cube")) // Prüfen, ob das Objekt den Tag "Cube" hat
            {
                currentCube = hit.transform; // Setze den aktuellen Cube auf das getroffene Objekt
            }
        }
        else
        {
            currentCube = null; // Kein Cube im Blickfeld
        }
    }

    // Methode, um den Cube zu rotieren
    void RotateCube()
    {
        // Rotieren um die X-Achse (auf und ab) mit der Pfeiltaste nach oben/unten
        if (Input.GetKey(KeyCode.UpArrow))
        {
            currentCube.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            currentCube.Rotate(Vector3.left * rotationSpeed * Time.deltaTime);
        }

        // Rotieren um die Y-Achse (links/rechts) mit der Pfeiltaste links/rechts
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            currentCube.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            currentCube.Rotate(Vector3.down * rotationSpeed * Time.deltaTime);
        }

        // Optional: Rotieren um die Z-Achse (vorwärts/rückwärts) mit "Q" und "R"
        if (Input.GetKey(KeyCode.Q))
        {
            currentCube.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.R))
        {
            currentCube.Rotate(Vector3.back * rotationSpeed * Time.deltaTime);
        }
    }
}