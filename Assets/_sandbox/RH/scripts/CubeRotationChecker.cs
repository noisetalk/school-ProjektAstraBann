using UnityEngine;

public class CubeRotationChecker : MonoBehaviour
{
    public Vector3 correctRotation; // Die richtige Rotation, die der Cube haben soll
    private bool isCorrectlyRotated = false;

    [SerializeField] bool debug = false;

    void Update()
    {
        CheckRotation(); // �berpr�fe in jedem Frame die Rotation des Cubes
    }

    // �berpr�ft, ob der Cube die richtige Rotation hat
    void CheckRotation()
    {
        Vector3 currentRotation = transform.eulerAngles;

        // Pr�fen, ob die aktuelle Rotation ungef�hr der korrekten Rotation entspricht
        if (Quaternion.Angle(transform.rotation, Quaternion.Euler(correctRotation)) <= 1f)
        {

        
            if (!isCorrectlyRotated)
            {
                isCorrectlyRotated = true;
                Debug.Log(gameObject.name + " hat die richtige Rotation erreicht!");
            }
        }
        else
        {
            if (isCorrectlyRotated)
            {
                isCorrectlyRotated = false;
                Debug.Log(gameObject.name + " hat die richtige Rotation verlassen.");
            }
        }
    }

    // Diese Methode wird vom Puzzle-Manager verwendet, um zu �berpr�fen, ob der Cube korrekt gedreht ist
    public bool IsCorrectlyRotated()
    {
        return isCorrectlyRotated;
    }
}