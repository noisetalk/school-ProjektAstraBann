using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    // Die Plattform, die durch die Druckplatte gesteuert wird
    public ObjectMoving platform;
    // Liste der Objekte, die die Platte aktivieren können
    public List<GameObject> activatingObjects = new List<GameObject>();
    // Renderer zum Ändern der Farbe der Platte
    public Renderer plateRenderer;

    // Anzahl der gültigen Objekte, die sich auf der Platte befinden
    private int objectsOnPlate = 0;

    private void Start()
    {
        // Aktualisiere die Plattenfarbe beim Start
        UpdatePlateColor();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prüfe, ob das Objekt in der Liste der aktivierenden Objekte ist
        if (activatingObjects.Contains(other.gameObject))
        {
            objectsOnPlate++; // Erhöhe die Anzahl der Objekte auf der Platte
            if (objectsOnPlate == 1 && platform != null)
            {
                platform.SetMovementActive(true); // Aktiviere die Plattform
            }
            UpdatePlateColor(); // Aktualisiere die Farbe der Platte
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Prüfe, ob das Objekt in der Liste der aktivierenden Objekte ist
        if (activatingObjects.Contains(other.gameObject))
        {
            objectsOnPlate--; // Verringere die Anzahl der Objekte auf der Platte
            if (objectsOnPlate == 0 && platform != null)
            {
                platform.SetMovementActive(false); // Deaktiviere die Plattform
            }
            UpdatePlateColor(); // Aktualisiere die Farbe der Platte
        }
    }

    // Methode zur Aktualisierung der Plattenfarbe basierend auf dem Status der Plattform
    private void UpdatePlateColor()
    {
        if (plateRenderer != null && platform != null)
        {
            // Ändere die Farbe der Platte abhängig vom Bewegungsstatus der Plattform
            plateRenderer.material.color = platform.isMoving ? Color.white : Color.gray;
        }
    }
}
