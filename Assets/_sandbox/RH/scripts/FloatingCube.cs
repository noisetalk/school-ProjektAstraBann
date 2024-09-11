using UnityEngine;

public class FloatingCube : MonoBehaviour
{
    public float floatSpeed = 1.0f;  // Geschwindigkeit des Schwebens
    public float floatHeight = 0.5f; // Höhe des Schwebens
    public Vector3 rotationSpeed = new Vector3(15f, 30f, 45f); // Rotation in Grad pro Sekunde

    public Vector3 startPosition;
    //public Vector3 originPoint;
    //

    void Start()
    {
        // Speichere die Startposition des Cubes
        startPosition = transform.position;
        //originPoint =
    }

    void Update()
    {
        // Simuliere das Schweben durch Sinus-Funktion
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, startPosition.y + newY, startPosition.z);

        // Rotiert den Cube kontinuierlich
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    // Optionale Funktion für das manuelle Rotieren
    public void RotateCube(Vector3 rotation)
    {
        transform.Rotate(rotation);
    }
}