using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Подключает сброс достижений к старту новой игры в главном меню.
/// Не меняет MainMenuManager и другие чужие скрипты.
/// </summary>
public class AchievementNewGameResetter : MonoBehaviour
{
    [Header("Optional manual link")]
    public Button startNewGameButton;

    [Header("Auto search")]
    public bool autoFindButton = true;

    private bool listenerAdded;

    void Start()
    {
        TryConnect();
    }

    void Update()
    {
        if (!listenerAdded && autoFindButton)
            TryConnect();
    }

    void TryConnect()
    {
        if (startNewGameButton == null && autoFindButton)
            startNewGameButton = FindStartGameButton();

        if (startNewGameButton == null || listenerAdded)
            return;

        startNewGameButton.onClick.AddListener(ResetAchievementsForNewGame);
        listenerAdded = true;
    }

    Button FindStartGameButton()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            Text text = button.GetComponentInChildren<Text>(true);
            if (text == null)
                continue;

            string label = text.text.Trim().ToLowerInvariant();

            // В проекте эта кнопка находится на панели выбора сложности.
            // Она запускает именно новую игру после выбора сложности.
            if (label == "начать игру")
                return button;
        }

        return null;
    }

    void ResetAchievementsForNewGame()
    {
        AchievementManager.ResetSavedAchievements();
    }
}
