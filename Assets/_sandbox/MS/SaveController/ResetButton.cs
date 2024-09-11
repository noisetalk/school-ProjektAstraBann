using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetButton : MonoBehaviour
{
    // Radius für die Interaktion
    public float interactionRadius = 3f;

    // Gizmo-Farbe für den Interaktionsradius
    public Color gizmoColor = Color.yellow;

    private GameObject player;

    private void Start()
    {
        // Suche nach dem Spieler mit dem Tag "Player"
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        // Überprüfen, ob der Spieler innerhalb des Interaktionsradius ist
        if (Vector3.Distance(player.transform.position, transform.position) <= interactionRadius)
        {
            // Wenn der Spieler im Radius ist und "E" drückt, wird die Szene zurückgesetzt
            if (Input.GetKeyDown(KeyCode.E))
            {
                ResetScene();
            }
        }
    }

    // Methode zum Zurücksetzen der aktuellen Szene
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Zeichnet Gizmos zur Visualisierung des Interaktionsradius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
