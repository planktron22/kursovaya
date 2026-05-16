using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Прикрепи этот скрипт на панель уведомления конкурента в сцене
public class CompetitorEventPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;       // "Теневой Конкурент"
    public TextMeshProUGUI messageText;     // текст события
    public Button closeButton;             // кнопка X

    [Header("Цвета")]
    public Color sabotageColor = new Color(0.9f, 0.2f, 0.2f);  // красный для саботажа
    public Color helpColor     = new Color(0.2f, 0.75f, 0.3f); // зелёный для помощи

    public Image headerBackground; // фон заголовка — меняет цвет

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        gameObject.SetActive(false);
    }

    public void Show(string competitorName, string message, bool isSabotage)
    {
        gameObject.SetActive(true);

        if (titleText != null)
            titleText.text = competitorName;

        if (messageText != null)
            messageText.text = message;

        if (headerBackground != null)
            headerBackground.color = isSabotage ? sabotageColor : helpColor;
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        // Сообщаем UIManager что панель закрыта
        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
            ui.isPanelOpen = false;
    }
}
