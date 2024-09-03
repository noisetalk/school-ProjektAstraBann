using UnityEngine;

public class GameStartTrigger : MonoBehaviour
{
    public CupGameController gameController; // Referenz zum Hütchenspiel-Controller

    private void Start()
    {
        gameController = GetComponent<CupGameController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Überprüfen, ob der Spieler den Trigger betritt
        {
            
            gameController.StartGame(); // Spiel starten, wenn der Spieler im Bereich ist
        }
    }
}