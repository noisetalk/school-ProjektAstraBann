using UnityEngine;

public class CubePuzzleManager : MonoBehaviour
{
    [SerializeField] private CubeRotationChecker[] cubes;    // Array mit allen Cubes
    public Vector3[] targetPositions;      // Zielpositionen der Cubes nach Lösung des Puzzles
    public float moveSpeed = 2f;           // Bewegungsgeschwindigkeit
    private bool puzzleSolved = false;     // Status, ob das Puzzle gelöst ist

    void Update()
    {
        if (!puzzleSolved)
        {
            if (CheckPuzzleSolved())
            {
                puzzleSolved = true;
                Debug.Log("Das Puzzle ist gelöst! Die Cubes bewegen sich.");
            }
            else
            {
                Debug.Log("Das Puzzle ist noch nicht gelöst.");
            }
        }
        else
        {
            MoveCubesCloser();  // Wenn das Puzzle gelöst ist, bewege die Cubes
        }

        //TestMoveCubes();  // Zum Testen der Bewegungslogik
    }

    // Überprüft, ob alle Cubes richtig rotiert sind
    bool CheckPuzzleSolved()
    {
        foreach (CubeRotationChecker cube in cubes)
        {
            if (!cube.IsCorrectlyRotated())
            {
                return false;  // Wenn ein Cube nicht richtig rotiert ist, ist das Puzzle nicht gelöst
            }
        }
        return true;  // Alle Cubes sind richtig rotiert
    }

    // Bewege die Cubes schrittweise zu den Zielpositionen
    void MoveCubesCloser()
    {
        for (int i = 0; i < cubes.Length; i++)
        {
            //cubes[i].transform.position = Vector3.MoveTowards(cubes[i].transform.position, targetPositions[i], moveSpeed * Time.deltaTime);
            cubes[i].GetComponent<FloatingCube>().startPosition = Vector3.MoveTowards(cubes[i].GetComponent<FloatingCube>().startPosition, targetPositions[i], moveSpeed * Time.deltaTime);
        }
    }

    // Teste die Bewegung der Cubes durch Drücken der Taste "M"
    void TestMoveCubes()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MoveCubesCloser();
            Debug.Log("Testweise Cubes bewegt.");
        }
    }
}