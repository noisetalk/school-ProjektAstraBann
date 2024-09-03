using UnityEngine;

public class CupGameManager : MonoBehaviour
{
    public CupController[] cups; // Array von Becher-Controllern

    public void StartShuffling()
    {
        // Beispiel: Starte alle Becher gleichzeitig zu shuffeln
        foreach (CupController cup in cups)
        {
            cup.StartMoving(); // Ruft die Bewegung für jeden Becher auf
        }
    }
}