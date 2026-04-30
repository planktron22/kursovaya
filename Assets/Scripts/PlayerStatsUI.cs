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

    private CultureInfo culture = new CultureInfo("ru-RU");

    public void UpdateStats(PlayerStats stats)
    {
        balanceText.text = Format(stats.Balance) + " р.";
        netIncomeText.text = Format(stats.NetIncome) + " р.";
        incomeText.text = Format(stats.Income) + " р.";
        lossText.text = Format(stats.Loss) + " р.";
        freeTimeText.text = Format(stats.FreeTime) + " ч.";
        healthText.text = Format(stats.Health) + "/100";
        moodText.text = Format(stats.Mood)+"/100";
        ageText.text = Format(stats.Age) + " лет";
    }

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }
}