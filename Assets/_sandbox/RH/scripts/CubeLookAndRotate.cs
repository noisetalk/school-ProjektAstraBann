using UnityEngine;

public class CubeLookAndRotate : MonoBehaviour
{
    public float rotationSpeed = 300f;      // Geschwindigkeit der Rotation
    public float raycastDistance = 10f;     // Wie weit der Raycast reicht
    private Quaternion targetRotation;      // Zielrotation nach jeder Eingabe
    private bool isRotating = false;        // Prüfen, ob der Cube gerade rotiert
    private Renderer cubeRenderer;

    void Start()
    {
        targetRotation = transform.rotation; // Setze die aktuelle Rotation als Startziel
        cubeRenderer = GetComponent<Renderer>();  // Zugriff auf den Renderer des Cubes
    }

    void Update()
    {
        // Raycast von der Kamera in Richtung der Mitte des Bildschirms
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        // Überprüfen, ob der Raycast den Cube trifft
        if (Physics.Raycast(ray, out hit, raycastDistance))
        {
            if (hit.collider.gameObject == gameObject && !isRotating)
            {
                // Der Spieler schaut den Cube an und kann ihn drehen
                cubeRenderer.material.color = Color.green; // Ändere die Farbe, um zu zeigen, dass der Cube ausgewählt ist

                // Wenn der Spieler "E" drückt, drehe den Cube um die Y-Achse
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    RotateCube(Vector3.up);
                }
                // Wenn der Spieler "Q" drückt, drehe den Cube um die X-Achse
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    RotateCube(Vector3.right);
                }
                // Wenn der Spieler "R" drückt, drehe den Cube um die Z-Achse
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    RotateCube(Vector3.forward);
                }
            }
            else
            {
                // Der Spieler schaut nicht den Cube an, normale Farbe
                cubeRenderer.material.color = Color.white;
            }
        }

        // Führe die Rotation allmählich durch
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Wenn die Rotation fast abgeschlossen ist, stoppe sie
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
            }
        }
    }

    // Methode, um die Cube-Drehung um 90 Grad in eine bestimmte Richtung zu starten
    void RotateCube(Vector3 rotationAxis)
    {
        if (!isRotating)
        {
            targetRotation *= Quaternion.AngleAxis(90, rotationAxis);  // Setze die Zielrotation um 90 Grad
            isRotating = true; // Rotation beginnt
        }
    }
}