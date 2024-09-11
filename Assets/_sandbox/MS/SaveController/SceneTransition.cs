using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    // Name der Szene, zu der gewechselt werden soll
    public string sceneToLoad;

    // Ob das Speichern der Szenendaten aktiviert ist
    public bool saveSceneData = true;

    private void OnTriggerEnter(Collider other)
    {
        // Überprüfen, ob der Trigger vom Spieler aktiviert wurde
        if (other.CompareTag("Player"))
        {
            if (saveSceneData)
            {
                // Den Zustand der aktuellen Szene speichern
                SaveSceneState();
            }
            // Die neue Szene laden
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void SaveSceneState()
    {
        // Alle Objekte in der Szene finden
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.transform != null)
            {
                Vector3 position = obj.transform.position;
                string keyPrefix = SceneManager.GetActiveScene().name + "_" + obj.name;

                // Position jedes Objekts speichern
                PlayerPrefs.SetFloat(keyPrefix + "_x", position.x);
                PlayerPrefs.SetFloat(keyPrefix + "_y", position.y);
                PlayerPrefs.SetFloat(keyPrefix + "_z", position.z);

                // Debug-Nachricht ausgeben
                Debug.Log("Gespeichert: " + keyPrefix + " -> " + position);
            }
        }

        // Name der aktuellen Szene speichern
        PlayerPrefs.SetString("CurrentScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save(); // Alle Daten speichern
        Debug.Log("Szenenzustand gespeichert.");
    }
}
