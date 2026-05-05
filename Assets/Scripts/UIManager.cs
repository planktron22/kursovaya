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
    public GameObject opportunityPanel;
    public GameObject jobPanel;
    public GameObject businessPanel;
    public GameObject realtyPanel;
    public GameObject investPanel;

    public PlayerMovement playerMovement;
    public UnityEngine.UI.Button opportunityButton;

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

    public void ToggleOpportunity()
    {
        if (playerMovement == null) return;

        if (playerMovement.isMoving)
        {
            Debug.Log("Нельзя открыть во время движения");
            return;
        }

        if (playerMovement.GetCurrentTileType() != TileType.Empty)
        {
            Debug.Log("Можно открыть только на пустой клетке");
            return;
        }

        if (opportunityPanel.activeSelf)
        {
            opportunityPanel.SetActive(false);
            isPanelOpen = false;
        }
        else
        {
            opportunityPanel.SetActive(true);
            isPanelOpen = true;
        }
    }

    public void HideAll()
    {
        if (communityPanel) communityPanel.SetActive(false);
        if (studyingPanel) studyingPanel.SetActive(false);
        if (luckPanel) luckPanel.SetActive(false);
        if (unluckPanel) unluckPanel.SetActive(false);
        if (ultraLuckPanel) ultraLuckPanel.SetActive(false);
        if (ultraUnluckPanel) ultraUnluckPanel.SetActive(false);

        isPanelOpen = false;
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }


    public void ToggleJobs()
    {
        if (jobPanel.activeSelf)
        {
            jobPanel.SetActive(false);
            isPanelOpen = false;
        }
        else
        {
            jobPanel.SetActive(true);
            isPanelOpen = true;
        }
    }

    public void ToggleBusinesses()
    {
        if (businessPanel.activeSelf)
        {
            businessPanel.SetActive(false);
            isPanelOpen = false;
        }
        else
        {
            businessPanel.SetActive(true);
            isPanelOpen = true;
        }
    }
    public void ToggleRealty()
    {
        if (realtyPanel.activeSelf)
        {
            realtyPanel.SetActive(false);
            isPanelOpen = false;
        }
        else
        {
            realtyPanel.SetActive(true);
            isPanelOpen = true;
        }
    }

    public void ToggleInvest()
    {
        if (investPanel.activeSelf)
        {
            investPanel.SetActive(false);
            isPanelOpen = false;
        }
        else
        {
            investPanel.SetActive(true);
            isPanelOpen = true;
        }
    }

    void Update()
    {
        if (playerMovement == null || opportunityButton == null)
            return;

        bool canOpen =
            !playerMovement.isMoving &&
            playerMovement.GetCurrentTileType() == TileType.Empty &&
            !isPanelOpen;

    }
}