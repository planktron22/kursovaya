using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject difficultyPanel;
    public GameObject settingsPanel;

    public Text descriptionText;
    public Button startGameButton;

    private int selectedDifficulty = -1;

    // --- Панели ---
    public void OpenDifficulty()
    {
        mainPanel.SetActive(false);
        difficultyPanel.SetActive(true);

        descriptionText.text = "Выберите сложность";
        startGameButton.interactable = false;
    }

    public void OpenSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);

    }

    public void BackToMain()
    {
        settingsPanel.SetActive(false);
        difficultyPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // --- Выбор сложности ---

    public void SelectEasy()
    {
        selectedDifficulty = 0;
        descriptionText.text =
            "Лёгкая сложность:\n" +
            "- Высокий стартовый капитал\n" +
            "- Меньше штрафов\n" +
            "- Проще поддерживать настроение";

        startGameButton.interactable = true;
    }

    public void SelectMedium()
    {
        selectedDifficulty = 1;
        descriptionText.text =
            "Средняя сложность:\n" +
            "- Сбалансированные параметры\n" +
            "- Умеренные расходы\n" +
            "- Стандартный уровень риска";

        startGameButton.interactable = true;
    }

    public void SelectHard()
    {
        selectedDifficulty = 2;
        descriptionText.text =
            "Высокая сложность:\n" +
            "- Низкий стартовый капитал\n" +
            "- Высокие расходы\n" +
            "- Быстро падает настроение и здоровье";

        startGameButton.interactable = true;
    }

    // --- Старт игры ---

    public void StartGame()
    {
        if (selectedDifficulty == -1)
            return;

        PlayerPrefs.SetInt("Difficulty", selectedDifficulty);
        SceneManager.LoadScene("SampleScene");
    }

    public void LoadGame()
    {
        SaveLoadManager.LoadGameFromMenu();
    }

    // --- Выход ---

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}