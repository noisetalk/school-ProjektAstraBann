using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public CubeScript[] cubes; // Array der sechs Steine
    public DoorController doorController; // Referenz zur T�rsteuerung

    void Update()
    {
        // �berpr�fe, ob alle Steine korrekt gestapelt sind
        if (AreAllCubesCorrectlyPlaced())
        {
            // Puzzle gel�st, T�r �ffnen
            doorController.PuzzleCompleted();
        }
    }

    // Methode, um zu �berpr�fen, ob alle Steine korrekt gestapelt sind
    private bool AreAllCubesCorrectlyPlaced()
    {
        foreach (CubeScript cube in cubes)
        {
            if (!cube.IsCorrectlyPlaced())
            {
                return false; // Wenn ein Stein falsch platziert ist, ist das Puzzle noch nicht gel�st
            }
        }
        return true; // Alle Steine sind korrekt gestapelt
    }
}