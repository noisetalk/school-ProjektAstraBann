using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public Vector3 targetPosition; // Die richtige Position f�r diesen Stein
    public Vector3 targetRotation; // Die richtige Rotation (Eulerwinkel) f�r diesen Stein
    public float positionTolerance = 0.1f; // Toleranz f�r Positions�berpr�fung
    public float rotationTolerance = 5.0f; // Toleranz f�r Rotations�berpr�fung (in Grad)

    // Methode, um zu �berpr�fen, ob dieser Stein korrekt gestapelt ist
    public bool IsCorrectlyPlaced()
    {
        // �berpr�fe, ob die Position des Steins innerhalb der Toleranz liegt
        bool positionCorrect = Vector3.Distance(transform.position, targetPosition) <= positionTolerance;

        // �berpr�fe, ob die Rotation des Steins innerhalb der Toleranz liegt
        bool rotationCorrect = Vector3.Distance(transform.eulerAngles, targetRotation) <= rotationTolerance;

        return positionCorrect && rotationCorrect;
    }


// Start is called before the first frame update
void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
