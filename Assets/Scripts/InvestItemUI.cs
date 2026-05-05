using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class InvestItemUI : MonoBehaviour
{
    public Text titleText;
    public Text descriptionText;
    public Text costText;
    public Text amountText;
    public Text riskText;

    public InputField amountInput;

    public Button buyButton;
    public Button sellButton;

    private OpportunityData investData;
    private PlayerStats player;

    private CultureInfo culture = new CultureInfo("ru-RU");

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }

    public void Setup(OpportunityData data, PlayerStats stats)
    {
        investData = data;
        player = stats;

        UpdateInfo();
    }

    public void UpdateInfo()
    {
        PlayerInvest invest = player.GetInvest(investData);

        if (invest == null)
            return;

        titleText.text = invest.title;
        costText.text = "Цена: " + Format(invest.currentCost) + " р.";
        amountText.text = "У вас: " + invest.amount;
        riskText.text = "Риск: " + GetRiskName(investData.investRisk);
    }

    string GetRiskName(int risk)
    {
        switch (risk)
        {
            case 1: return "минимальный";
            case 2: return "небольшой";
            case 3: return "средний";
            case 4: return "высокий";
            case 5: return "максимальный";
            default: return "неизвестный";
        }
    }

    int GetInputAmount()
    {
        int amount;

        if (!int.TryParse(amountInput.text, out amount))
        {
            Debug.Log("Введите корректное количество акций");
            return 0;
        }

        return amount;
    }

    public void OnBuyClicked()
    {
        int amount = GetInputAmount();

        if (amount <= 0)
            return;

        player.BuyInvest(investData, amount);
        UpdateInfo();
    }

    public void OnSellClicked()
    {
        int amount = GetInputAmount();

        if (amount <= 0)
            return;

        player.SellInvest(investData, amount);
        UpdateInfo();
    }
}