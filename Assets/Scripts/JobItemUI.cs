using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class JobItemUI : MonoBehaviour
{
    public Text titleText;
    public Text descriptionText;
    public Text incomeText;
    public Text timeText;
    public Text bonusText;
    public Text totalIncomeText;

    public Button applyButton;   
    public Button fireButton;    

    private OpportunityData job;
    private PlayerStats player;

    private CultureInfo culture = new CultureInfo("ru-RU");

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }

    public void Setup(OpportunityData data, PlayerStats stats)
    {
        job = data;
        player = stats;

        int totalIncome = data.jobIncomePerHour * data.jobHours;

        titleText.text = data.title;
        descriptionText.text = data.description;

        incomeText.text = "Доход в час: " + Format(data.jobIncomePerHour) + " р/ч";
        timeText.text = "Часы: " + Format(data.jobHours) + " ч";
        bonusText.text = $"Бонус: {Format(data.jobBonusMin)}-{Format(data.jobBonusMax)}";
        totalIncomeText.text = "Доход в период: " + Format(totalIncome) + " р";

        UpdateButtons();
    }

    void UpdateButtons()
    {
        bool hasJob = player.HasJob(job);

        applyButton.gameObject.SetActive(!hasJob);
        fireButton.gameObject.SetActive(hasJob);

        if (hasJob)
        {
            PlayerActive j = player.GetJob(job);
        }
    }

    public void OnApplyClicked()
    {
        player.ApplyJob(job);
        UpdateButtons();
    }

    public void OnFireClicked()
    {
        PlayerActive jobToRemove = player.GetJob(job);

        if (jobToRemove != null)
        {
            if (jobToRemove.workedPeriods < 3)
            {
                Debug.Log("Нельзя уволиться раньше чем через 3 периода");
                return;
            }

            player.RemoveJob(jobToRemove);
        }

        UpdateButtons();
    }
}