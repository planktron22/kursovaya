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

    private CultureInfo culture = new CultureInfo("ru-RU");

    public void UpdateStats(PlayerStats stats)
    {
        balanceText.text = Format(stats.Balance) + " ð.";
        netIncomeText.text = Format(stats.NetIncome) + " ð.";
        incomeText.text = Format(stats.Income) + " ð.";
        lossText.text = Format(stats.Loss) + " ð.";
        freeTimeText.text = Format(stats.FreeTime) + " ÷.";
        healthText.text = Format(stats.Health) + "/100";
        moodText.text = Format(stats.Mood)+"/100";
    }

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }
}