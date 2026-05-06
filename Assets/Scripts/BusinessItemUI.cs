using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class BusinessItemUI : MonoBehaviour
{
    public Text titleText;
    public Text descriptionText;
    public Text costText;
    public Text incomeText;
    public Text delayText;
    public Text timeText;
    public Text requiredSkillsText;

    public Button buyButton;
    public Button sellButton;

    private OpportunityData business;
    private PlayerStats player;

    private CultureInfo culture = new CultureInfo("ru-RU");

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }

    public void Setup(OpportunityData data, PlayerStats stats)
    {
        business = data;
        player = stats;

        titleText.text = data.title;
        descriptionText.text = data.description;

        costText.text = "Вложения: " + Format(data.businessCost) + " р.";
        incomeText.text = "Доход: " + Format(data.businessIncome) + " р.";
        delayText.text = "Запуск: " + data.businessStartTime + " периодов";
        timeText.text = "Время: " + data.businessTimeCost + " ч.";

        UpdateButtons();
    }

    void UpdateButtons()
    {
        PlayerActive activeBusiness = player.GetJob(business);

        bool hasBusiness = activeBusiness != null;

        buyButton.gameObject.SetActive(!hasBusiness);
        sellButton.gameObject.SetActive(hasBusiness);
    }

    public void OnBuyClicked()
    {
        player.ApplyBusiness(business);
        UpdateButtons();
    }

    public void OnSellClicked()
    {
        PlayerActive activeBusiness = player.GetJob(business);

        if (activeBusiness == null)
            return;

        if (activeBusiness.currentDelay < activeBusiness.startDelay)
        {
            int remaining = activeBusiness.startDelay - activeBusiness.currentDelay;

            Debug.Log(
                "Нельзя продать бизнес раньше запуска. " +
                "Осталось периодов до запуска: " + remaining
            );

            return;
        }

        player.SellBusiness(activeBusiness);
        UpdateButtons();
    }
}