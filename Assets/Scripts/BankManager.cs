using UnityEngine;
using UnityEngine.UI;
using System.Globalization;

public class BankManager : MonoBehaviour
{
    public PlayerStats player;

    public InputField depositAmountInput;
    public Slider depositYearsSlider;
    public Text depositYearsText;
    public Text depositResultText;

    public InputField creditAmountInput;
    public Slider creditYearsSlider;
    public Text creditYearsText;
    public Text creditResultText;

    private CultureInfo culture = new CultureInfo("ru-RU");

    void OnEnable()
    {
        UpdateDepositPreview();
        UpdateCreditPreview();
    }

    void Start()
    {
        depositYearsSlider.wholeNumbers = true;
        depositYearsSlider.minValue = 1;
        depositYearsSlider.maxValue = 5;

        creditYearsSlider.wholeNumbers = true;
        creditYearsSlider.minValue = 1;
        creditYearsSlider.maxValue = 5;

        depositYearsSlider.onValueChanged.AddListener(delegate { UpdateDepositPreview(); });
        creditYearsSlider.onValueChanged.AddListener(delegate { UpdateCreditPreview(); });

        depositAmountInput.onValueChanged.AddListener(delegate { UpdateDepositPreview(); });
        creditAmountInput.onValueChanged.AddListener(delegate { UpdateCreditPreview(); });

        UpdateDepositPreview();
        UpdateCreditPreview();
    }

    string Format(int value)
    {
        return value.ToString("N0", culture);
    }

    int ReadAmount(InputField input)
    {
        int amount;
        if (!int.TryParse(input.text, out amount))
            return 0;

        return amount;
    }

    public void UpdateDepositPreview()
    {
        int years = Mathf.RoundToInt(depositYearsSlider.value);
        int amount = ReadAmount(depositAmountInput);

        int finalAmount = BankCalculator.CalculateDepositFinalAmount(amount, years);
        float rate = BankCalculator.GetDepositRate(years) * 100f;

        depositYearsText.text = "Срок: " + years + " г.";
        depositResultText.text =
            "Ставка: " + rate.ToString("0.#") + "% годовых\n" +
            "К получению: " + Format(finalAmount) + " р.";
    }

    public void UpdateCreditPreview()
    {
        int years = Mathf.RoundToInt(creditYearsSlider.value);
        int amount = ReadAmount(creditAmountInput);

        int finalPayment = BankCalculator.CalculateCreditFinalPayment(amount, years);
        int hypotheticalIncome = player.NetIncome * years * 6;

        creditYearsText.text = "Срок: " + years + " г.";
        creditResultText.text =
            "Ставка: " + (BankCalculator.GetCreditRate(years) * 100f).ToString("0.#") + "% годовых\n" +
            "К выплате: " + Format(finalPayment) + " р.\n" +
            "Прогнозируемый доход: " + Format(hypotheticalIncome) + " р.";
    }

    public void OpenDeposit()
    {
        int amount = ReadAmount(depositAmountInput);
        int years = Mathf.RoundToInt(depositYearsSlider.value);

        player.OpenDeposit(amount, years);

        UpdateDepositPreview();
    }

    public void OpenCredit()
    {
        int amount = ReadAmount(creditAmountInput);
        int years = Mathf.RoundToInt(creditYearsSlider.value);

        player.OpenCredit(amount, years);

        UpdateCreditPreview();
    }
}