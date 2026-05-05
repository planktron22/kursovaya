using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class PlayerStatsInfo : MonoBehaviour
{
    public Text balanceText;
    public Text netIncomeText;
    public Text incomeText;
    public Text lossText;
    public Text freeTimeText;
    public Text healthText;
    public Text moodText;
    public Text ageText;

    public Transform jobsIncomeContainer;
    public Transform jobsTimeContainer;

    public GameObject jobTextPrefab;

    private CultureInfo culture = new CultureInfo("ru-RU");

    public void UpdateStats(PlayerStats stats)
    {
        balanceText.text = Format(stats.Balance) + " р.";

        incomeText.text = Format(stats.TotalIncome) + " р.";
        lossText.text = Format(stats.TotalLoss) + " р.";
        netIncomeText.text = Format(stats.NetIncome) + " р.";

        freeTimeText.text = Format(stats.FreeTime) + " ч.";
        healthText.text = stats.Health + "/100";
        moodText.text = stats.Mood + "/100";
        ageText.text = stats.Age + " лет";

        UpdateIncomeList(stats);
        UpdateTimeList(stats);
    }

    void UpdateIncomeList(PlayerStats stats)
    {
        foreach (Transform child in jobsIncomeContainer)
            Destroy(child.gameObject);

        foreach (var job in stats.activeJobs)
        {
            GameObject obj = Instantiate(jobTextPrefab, jobsIncomeContainer);
            Text text = obj.GetComponent<Text>();

            if (job.isBusiness)
            {
                int remaining = job.startDelay - job.currentDelay;

                if (remaining > 0)
                {
                    text.text = $"{job.title} + 0 (старт через {remaining})";
                }
                else
                {
                    text.text = $"{job.title} + {Format(job.jobData.businessIncome)}";
                }
            }
            else
            {
                int income = stats.CalculateJobIncome(job.jobData);
                text.text = $"{job.title} + {Format(income)}";
            }
        }
    }

    void UpdateTimeList(PlayerStats stats)
    {
        foreach (Transform child in jobsTimeContainer)
            Destroy(child.gameObject);

        foreach (var job in stats.activeJobs)
        {
            GameObject obj = Instantiate(jobTextPrefab, jobsTimeContainer);
            Text text = obj.GetComponent<Text>();

            text.text = job.title + " - " + job.timeCost + " ч.";
        }
    }

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }
}