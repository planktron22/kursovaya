using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject difficultyPanel;

    // открыть выбор сложности
    public void OpenDifficulty()
    {
        mainPanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }

    // назад в меню
    public void BackToMain()
    {
        difficultyPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    // старт игры
    public void StartGame(int difficulty)
    {
        PlayerPrefs.SetInt("Difficulty", difficulty);
        SceneManager.LoadScene("SampleScene");
    }

    // выход
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}