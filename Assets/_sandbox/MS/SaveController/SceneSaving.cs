using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneStateManager : MonoBehaviour
{
    public bool loadSceneData = true;
    public Transform playerTransform;

    private void Start()
    {
        if (loadSceneData)
        {
            // Geladene Daten der Szene beim Starten laden
            LoadSceneData();
            // Die Startposition des Spielers wiederherstellen
            RestorePlayerPosition();
        }
    }

    private void LoadSceneData()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            string keyPrefix = SceneManager.GetActiveScene().name + "_" + obj.name;

            // Überprüfen, ob gespeicherte Daten für das Objekt existieren
            if (PlayerPrefs.HasKey(keyPrefix + "_x"))
            {
                float x = PlayerPrefs.GetFloat(keyPrefix + "_x");
                float y = PlayerPrefs.GetFloat(keyPrefix + "_y");
                float z = PlayerPrefs.GetFloat(keyPrefix + "_z");
                obj.transform.position = new Vector3(x, y, z);

                Debug.Log("Geladen: " + keyPrefix + " -> " + new Vector3(x, y, z));
            }
        }
    }

    private void RestorePlayerPosition()
    {
        if (playerTransform != null)
        {
            // Überprüfen, ob die Startposition des Spielers vorhanden ist
            if (PlayerPrefs.HasKey("PlayerStartPosition_x") && PlayerPrefs.HasKey("PlayerStartPosition_y") && PlayerPrefs.HasKey("PlayerStartPosition_z"))
            {
                float x = PlayerPrefs.GetFloat("PlayerStartPosition_x");
                float y = PlayerPrefs.GetFloat("PlayerStartPosition_y");
                float z = PlayerPrefs.GetFloat("PlayerStartPosition_z");
                playerTransform.position = new Vector3(x, y, z);

                Debug.Log("Startposition des Spielers wiederhergestellt auf: " + new Vector3(x, y, z));
            }
            else
            {
                Debug.LogWarning("Startposition des Spielers nicht gefunden.");
            }
        }
    }
}
