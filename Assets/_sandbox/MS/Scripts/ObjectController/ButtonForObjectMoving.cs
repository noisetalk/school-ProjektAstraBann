using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    // Liste der Plattformen, die gesteuert werden sollen
    public List<ObjectMoving> platforms = new List<ObjectMoving>();

    public bool activateOnStart = false; // Flagge für Aktivierung der Plattformen beim Start
    public Renderer buttonRenderer; // Renderer zum Ändern der Farbe des Knopfes
    public float activationRadius = 3f; // Der Radius, in dem der Knopf aktiviert werden kann

    private bool isActivated;
    private Transform player; // Referenz auf den Spieler

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // Spielerobjekt finden
        isActivated = activateOnStart; // Plattformstatus beim Start einstellen
        UpdateButtonColor(); // Aktualisiere die Knopffarbe beim Start

        if (activateOnStart)
        {
            ActivatePlatforms(); // Aktiviert die Plattformen beim Start, wenn aktiviert
        }
        else
        {
            DeactivatePlatforms(); // Deaktiviert die Plattformen beim Start, wenn nicht aktiviert
        }
    }

    void Update()
    {
        // Überprüfen, ob der Spieler sich im Aktivierungsradius befindet
        if (Vector3.Distance(transform.position, player.position) <= activationRadius)
        {
            // Überprüfen, ob die Taste E gedrückt wurde
            if (Input.GetKeyDown(KeyCode.E))
            {
                TogglePlatforms(); // Umschalten des Plattformzustands
            }
        }
    }

    // Methode zur Aktivierung aller Plattformen
    public void ActivatePlatforms()
    {
        foreach (var platform in platforms)
        {
            platform.SetMovementActive(true); // Bewegung der Plattform aktivieren
        }
        isActivated = true;
        UpdateButtonColor(); // Aktualisiere die Knopffarbe
    }

    // Methode zur Deaktivierung aller Plattformen
    public void DeactivatePlatforms()
    {
        foreach (var platform in platforms)
        {
            platform.SetMovementActive(false); // Bewegung der Plattform deaktivieren
        }
        isActivated = false;
        UpdateButtonColor(); // Aktualisiere die Knopffarbe
    }

    // Methode zum Umschalten des Zustands aller Plattformen
    public void TogglePlatforms()
    {
        isActivated = !isActivated;
        if (isActivated)
        {
            ActivatePlatforms();
        }
        else
        {
            DeactivatePlatforms();
        }
    }

    // Methode zur Aktualisierung der Knopffarbe
    private void UpdateButtonColor()
    {
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = isActivated ? Color.white : Color.gray;
        }
    }

    // Methode zur Anzeige des Aktivierungsradius im Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius); // Zeichne den Radius im Editor
    }
}
