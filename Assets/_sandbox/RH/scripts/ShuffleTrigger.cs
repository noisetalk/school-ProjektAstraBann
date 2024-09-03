using UnityEngine;

public class ShuffleTrigger : MonoBehaviour
{
    public CupGameManager cupGameManager; // Referenz zum CupGameManager-Skript

    private void OnTriggerEnter(Collider other)
    {
        // Überprüfe, ob der Spieler den Triggerbereich betreten hat
        if (other.CompareTag("Player"))
        {
            // Startet das Shuffeln, wenn der Spieler in den Triggerbereich eintritt
            cupGameManager.StartShuffling();
        }
    }
}