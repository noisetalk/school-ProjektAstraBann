// Geschrieben von: A.Zimnicki

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController
{
    // Methode zum Laden einer Szene anhand ihres Präfixes
    public void LoadScene(string prefix)
    {
        // Iteriere durch alle Szenen in den Build-Einstellungen
        // WICHTIG: NUR SZENEN, DIE IN DER BE-Liste SIND, KÖNNEN DURCH DIESE METHODE GELADEN WERDEN
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            // Setzt den Szenenpfad anhand des Build-Index
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            // Extrahiert den Szenennamen aus dem Szenenpfad
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            // Überprüft, ob der Szenenname mit dem angegebenen Präfix beginnt
            if (sceneName.StartsWith(prefix))
            {
                // Lädt die gefundene Szene (yay!)
                SceneManager.LoadScene(sceneName);
                return;
            }
        }

        // Falls keine passende Szene gefunden wurde, gib einen Fehler aus
        Debug.LogError($"Keine Szene mit Präfix '{prefix}' gefunden! Auf Tippfehler überprüfen und BE-Liste überprüfen.");
    }
    
    // Backup-Methode zum Laden einer Szene anhand ihres Namens, ohne Präfix-Check
    // Obsolet, wenn Präfix-Check korrekt funktioniert, aber sicher ist sicher
    public void LoadSceneAlt(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

}
 /*
  Anwendung:
  1: SceneController.cs zu einem GameObject hinzufügen (wichtig, da MonoBehaviour)
  2: SceneController im ausführenden Script referenzieren
  3: SceneController.LoadScene("Präfix") aufrufen, um eine Szene zu laden
  
 public class BEISPIEL : MonoBehaviour
{
    public SceneController sceneController;

    void Start()
    {
        sceneController.LoadScene("A12");
    }
}
*/