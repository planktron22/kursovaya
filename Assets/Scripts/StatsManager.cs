using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public GameObject MainStatsPanel;
    public GameObject IncomePanel;
    public GameObject LossPanel;
    public GameObject FreeTimePanel;
    public GameObject HealthPanel;
    public GameObject MoodPanel;

    public void OpenPanel(GameObject panel)
    {
        HideAll();
        panel.SetActive(true);
    }

    public void BackToMain()
    {
        HideAll();
        MainStatsPanel.SetActive(true);
    }

    void HideAll()
    {
        MainStatsPanel.SetActive(false);
        IncomePanel.SetActive(false);
        LossPanel.SetActive(false);
        FreeTimePanel.SetActive(false);
        HealthPanel.SetActive(false);
        MoodPanel.SetActive(false);
    }
}