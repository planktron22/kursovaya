using UnityEngine;
using UnityEngine.UI;
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
    public GameObject settingsPanel;

    public PlayerMovement playerMovement;
    public UnityEngine.UI.Button opportunityButton;

    public bool isPanelOpen = false;

    public GameObject bankPanel;

    [Header("Luck Event Texts")]
    public Text luckTitleText;
    public Text luckDescriptionText;

    public Text unluckTitleText;
    public Text unluckDescriptionText;

    public Text ultraLuckTitleText;
    public Text ultraLuckDescriptionText;

    public Text ultraUnluckTitleText;
    public Text ultraUnluckDescriptionText;

    [Header("Event Colors")]
    public Color goodEventColor = new Color(0.2f, 0.8f, 0.3f);
    public Color badEventColor = new Color(0.9f, 0.2f, 0.2f);

    [Header("Community Texts")]
    public Text communityResultTitleText;
    public Text communityResultDescriptionText;

    [Header("Community Colors")]
    public Color communitySuccessColor = new Color(0.2f, 0.8f, 0.3f);
    public Color communityFailColor = new Color(0.9f, 0.2f, 0.2f);

    // ─── Панель события конкурента ───
    public CompetitorEventPanel competitorEventPanel;

    public void ShowCompetitorEvent(string competitorName, string title, string description, bool isSabotage)
    {
        if (competitorEventPanel == null) return;

        competitorEventPanel.Show(competitorName, title, description, isSabotage);
        isPanelOpen = true;
    }

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

    public void ToggleSettings()
    {
        if (settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            isPanelOpen = false;
        }
        else
        {
            settingsPanel.SetActive(true);
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

        PlayerStats stats = FindObjectOfType<PlayerStats>();
        bool debtMode = stats != null && stats.debtMode;

        if (!debtMode && playerMovement.GetCurrentTileType() != TileType.Empty)
        {
            Debug.Log("Меню доступно только на пустой клетке");
            return;
        }

        opportunityPanel.SetActive(!opportunityPanel.activeSelf);
        isPanelOpen = opportunityPanel.activeSelf;
    }

    public void HideAll()
    {
        if (communityPanel)   communityPanel.SetActive(false);
        if (studyingPanel)    studyingPanel.SetActive(false);
        if (luckPanel)        luckPanel.SetActive(false);
        if (unluckPanel)      unluckPanel.SetActive(false);
        if (ultraLuckPanel)   ultraLuckPanel.SetActive(false);
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

    public void ToggleBank()
    {
        if (playerMovement == null) return;

        if (playerMovement.isMoving)
        {
            Debug.Log("Нельзя открыть банк во время движения");
            return;
        }

        PlayerStats stats = FindObjectOfType<PlayerStats>();
        bool debtMode = stats != null && stats.debtMode;

        if (!debtMode && playerMovement.GetCurrentTileType() != TileType.Empty)
        {
            Debug.Log("Банк доступен только на пустой клетке");
            return;
        }

        bankPanel.SetActive(!bankPanel.activeSelf);
        isPanelOpen = bankPanel.activeSelf;
    }

    public void ShowRandomEventInfo(TileType tileType, string title, string description)
    {
        switch (tileType)
        {
            case TileType.Luck:
                if (luckTitleText != null)
                {
                    luckTitleText.text = title;
                    luckTitleText.color = goodEventColor;
                }

                if (luckDescriptionText != null)
                    luckDescriptionText.text = description;

                break;

            case TileType.Unluck:
                if (unluckTitleText != null)
                {
                    unluckTitleText.text = title;
                    unluckTitleText.color = badEventColor;
                }

                if (unluckDescriptionText != null)
                    unluckDescriptionText.text = description;

                break;

            case TileType.UltraLuck:
                if (ultraLuckTitleText != null)
                {
                    ultraLuckTitleText.text = title;
                    ultraLuckTitleText.color = goodEventColor;
                }

                if (ultraLuckDescriptionText != null)
                    ultraLuckDescriptionText.text = description;

                break;

            case TileType.UltraUnluck:
                if (ultraUnluckTitleText != null)
                {
                    ultraUnluckTitleText.text = title;
                    ultraUnluckTitleText.color = badEventColor;
                }

                if (ultraUnluckDescriptionText != null)
                    ultraUnluckDescriptionText.text = description;

                break;
        }
    }

    public void ShowCommunityResult(bool success, string personName)
    {
        if (communityResultTitleText != null)
        {
            if (success)
            {
                communityResultTitleText.text = "Успешное знакомство";
                communityResultTitleText.color = communitySuccessColor;
            }
            else
            {
                communityResultTitleText.text = "Знакомство не удалось";
                communityResultTitleText.color = communityFailColor;
            }
        }

        if (communityResultDescriptionText != null)
        {
            communityResultDescriptionText.text = personName;
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
