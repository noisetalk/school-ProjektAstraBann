using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public CubeScript[] cubes; // Array der sechs Steine
    public DoorController doorController; // Referenz zur Türsteuerung

    void Update()
    {
        // Überprüfe, ob alle Steine korrekt gestapelt sind
        if (AreAllCubesCorrectlyPlaced())
        {
            // Puzzle gelöst, Tür öffnen
            doorController.PuzzleCompleted();
        }
    }

    // Methode, um zu überprüfen, ob alle Steine korrekt gestapelt sind
    private bool AreAllCubesCorrectlyPlaced()
    {
        foreach (CubeScript cube in cubes)
        {
            if (!cube.IsCorrectlyPlaced())
            {
                return false; // Wenn ein Stein falsch platziert ist, ist das Puzzle noch nicht gelöst
            }
        }
        return true; // Alle Steine sind korrekt gestapelt
    }
}