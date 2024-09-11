using UnityEngine;
using TMPro; // TextMeshPro verwenden

public class TooltipTrigger : MonoBehaviour
{
    // Textfelder mit TextMeshPro
    public TextMeshProUGUI tooltipTitle;
    public TextMeshProUGUI tooltipDescription;

    // Der Titel und die Beschreibung, die für diese Szene angezeigt werden
    [Header("Tooltip Inhalt")]
    public string title = "Titel";
    [TextArea(3, 10)]
    public string description = "Beschreibung des Objekts oder der Situation.";

    // Radius der Sphäre um den ausgewählten Objekt
    public float sphereRadius = 5f;

    // Der Spieler
    public Transform player;

    // Объект, вокруг которого находится сфера
    public GameObject targetObject;

    // UI-элементы для подсказки
    public GameObject tooltipUI;
    public GameObject tooltipBackground;

    // Время задержки перед показом подсказки
    [Header("Einstellungen")]
    public float delay = 1f; // задержка в секундах

    private float timeEnteredZone;
    private bool playerIsInRange = false;
    private bool tooltipShown = false;

    private void Start()
    {
        // В начале текстовая панель скрыта
        tooltipUI.SetActive(false);
        tooltipBackground.SetActive(false);
    }

    private void Update()
    {
        if (targetObject == null)
        {
            Debug.LogError("Kein Zielobjekt festgelegt.");
            return;
        }

        // Проверка, находится ли игрок в пределах радиуса
        Collider[] hitColliders = Physics.OverlapSphere(targetObject.transform.position, sphereRadius);
        bool isPlayerInRange = false;

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.transform == player)
            {
                isPlayerInRange = true;
                break;
            }
        }

        // Если игрок входит в зону
        if (isPlayerInRange && !playerIsInRange)
        {
            playerIsInRange = true;
            timeEnteredZone = Time.time; // Запоминаем время входа в зону
        }

        // Если игрок выходит из зоны
        if (!isPlayerInRange && playerIsInRange)
        {
            playerIsInRange = false;
            tooltipShown = false; // Сброс состояния показа подсказки
            tooltipUI.SetActive(false);  // Скрываем подсказку
            tooltipBackground.SetActive(false);  // Скрываем фон
        }

        // Проверяем, прошло ли достаточное время для показа подсказки
        if (playerIsInRange && !tooltipShown)
        {
            if (Time.time - timeEnteredZone >= delay)
            {
                // Обновление текста и показ панели с фоном
                tooltipTitle.text = title;
                tooltipDescription.text = description;

                tooltipUI.SetActive(true);
                tooltipBackground.SetActive(true);
                tooltipShown = true; // Помечаем, что подсказка уже показана
            }
        }
    }

    // Метод для отображения сферы в редакторе
    private void OnDrawGizmos()
    {
        if (targetObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetObject.transform.position, sphereRadius);
        }
    }
}
