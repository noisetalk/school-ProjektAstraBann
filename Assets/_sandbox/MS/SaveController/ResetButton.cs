using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetButton : MonoBehaviour
{
    // Radius f�r die Interaktion
    public float interactionRadius = 3f;

    // Gizmo-Farbe f�r den Interaktionsradius
    public Color gizmoColor = Color.yellow;

    private GameObject player;

    private void Start()
    {
        // Suche nach dem Spieler mit dem Tag "Player"
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        // �berpr�fen, ob der Spieler innerhalb des Interaktionsradius ist
        if (Vector3.Distance(player.transform.position, transform.position) <= interactionRadius)
        {
            // Wenn der Spieler im Radius ist und "E" dr�ckt, wird die Szene zur�ckgesetzt
            if (Input.GetKeyDown(KeyCode.E))
            {
                ResetScene();
            }
        }
    }

    // Methode zum Zur�cksetzen der aktuellen Szene
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
