using UnityEngine;

public class PlayerPositionManager : MonoBehaviour
{
    // Der Transform des Spielers, dessen Position gespeichert werden soll
    public Transform playerTransform;
    public Rigidbody playerRigidbody; // Optional, falls Rigidbody verwendet wird

    private void Start()
    {
        // Überprüfen, ob die Startposition des Spielers bereits gespeichert ist
        if (PlayerPrefs.HasKey("PlayerStartPosition_x") && PlayerPrefs.HasKey("PlayerStartPosition_y") && PlayerPrefs.HasKey("PlayerStartPosition_z"))
        {
            // Wenn die Startposition gespeichert ist, diese Position wiederherstellen
            float x = PlayerPrefs.GetFloat("PlayerStartPosition_x");
            float y = PlayerPrefs.GetFloat("PlayerStartPosition_y");
            float z = PlayerPrefs.GetFloat("PlayerStartPosition_z");
            playerTransform.position = new Vector3(x, y, z);

            // Optional: Gleitfähigkeit und andere Physik-Parameter zurücksetzen
            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
                playerRigidbody.Sleep(); // Verhindert Bewegung, solange Position nicht aktualisiert ist
            }
        }
        else
        {
            // Startposition des Spielers speichern, wenn die Szene zum ersten Mal geladen wird
            SaveInitialPosition();
        }
    }

    private void SaveInitialPosition()
    {
        if (playerTransform != null)
        {
            Vector3 startPosition = playerTransform.position;
            PlayerPrefs.SetFloat("PlayerStartPosition_x", startPosition.x);
            PlayerPrefs.SetFloat("PlayerStartPosition_y", startPosition.y);
            PlayerPrefs.SetFloat("PlayerStartPosition_z", startPosition.z);
            PlayerPrefs.Save(); // Daten speichern
            Debug.Log("Startposition des Spielers gespeichert.");
        }
    }

    private void OnDestroy()
    {
        // Löscht gespeicherte Startposition beim Wechsel der Szene
        PlayerPrefs.DeleteKey("PlayerStartPosition_x");
        PlayerPrefs.DeleteKey("PlayerStartPosition_y");
        PlayerPrefs.DeleteKey("PlayerStartPosition_z");
        Debug.Log("Startposition des Spielers gelöscht.");
    }
}
