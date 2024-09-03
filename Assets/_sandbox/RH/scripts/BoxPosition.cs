using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public Vector3 targetPosition; // Die richtige Position für diesen Stein
    public Vector3 targetRotation; // Die richtige Rotation (Eulerwinkel) für diesen Stein
    public float positionTolerance = 0.1f; // Toleranz für Positionsüberprüfung
    public float rotationTolerance = 5.0f; // Toleranz für Rotationsüberprüfung (in Grad)

    // Methode, um zu überprüfen, ob dieser Stein korrekt gestapelt ist
    public bool IsCorrectlyPlaced()
    {
        // Überprüfe, ob die Position des Steins innerhalb der Toleranz liegt
        bool positionCorrect = Vector3.Distance(transform.position, targetPosition) <= positionTolerance;

        // Überprüfe, ob die Rotation des Steins innerhalb der Toleranz liegt
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
