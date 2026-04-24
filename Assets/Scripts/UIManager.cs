using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject communityPanel;
    public GameObject studyingPanel;
    public GameObject luckPanel;
    public GameObject unluckPanel;
    public GameObject ultraLuckPanel;
    public GameObject ultraUnluckPanel;
    public GameObject pausePanel;

    public bool isPanelOpen = false;
    public void ShowPanel(TileType type)
    {
        HideAll();

        isPanelOpen = true;

        switch (type)
        {
            case TileType.Community:
                communityPanel.SetActive(true);
                break;

            case TileType.Studying:
                studyingPanel.SetActive(true);
                break;

            case TileType.Luck:
                luckPanel.SetActive(true);
                break;

            case TileType.Unluck:
                unluckPanel.SetActive(true);
                break;

            case TileType.UltraLuck:
                ultraLuckPanel.SetActive(true);
                break;

            case TileType.UltraUnluck:
                ultraUnluckPanel.SetActive(true);
                break;
        }
    }

    public void TogglePause()
    {
        if (pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
            isPanelOpen = false;
        }
        else
        {
            pausePanel.SetActive(true);
            isPanelOpen = true;
        }
    }

    public void HideAll()
    {
        communityPanel.SetActive(false);
        studyingPanel.SetActive(false);
        luckPanel.SetActive(false);
        unluckPanel.SetActive(false);
        ultraLuckPanel.SetActive(false);
        ultraUnluckPanel.SetActive(false);

        isPanelOpen = false;
    }


    // назад в меню
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}