using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class RealtyItemUI : MonoBehaviour
{
    public Text titleText;
    public Text descriptionText;
    public Text costText;
    public Text incomeText;
    public Text timeText;

    public Button buyButton;
    public Button sellButton;

    private OpportunityData realty;
    private PlayerStats player;

    private CultureInfo culture = new CultureInfo("ru-RU");

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }

    public void Setup(OpportunityData data, PlayerStats stats)
    {
        realty = data;
        player = stats;

        titleText.text = data.title;
        descriptionText.text = data.description;
        costText.text = "Стоимость: " + Format(data.realtyCost) + " р.";
        incomeText.text = "Доход: " + Format(data.realtyIncome) + " р.";
        timeText.text = "Время: " + data.realtyTimeCost + " ч.";

        UpdateButtons();
    }

    void UpdateButtons()
    {
        PlayerActive activeRealty = player.GetJob(realty);

        bool hasRealty = activeRealty != null;

        buyButton.gameObject.SetActive(!hasRealty);
        sellButton.gameObject.SetActive(hasRealty);
    }

    public void OnBuyClicked()
    {
        player.ApplyRealty(realty);
        UpdateButtons();
    }

    public void OnSellClicked()
    {
        PlayerActive activeRealty = player.GetJob(realty);

        if (activeRealty == null)
            return;

        player.SellRealty(activeRealty);
        UpdateButtons();
    }
}