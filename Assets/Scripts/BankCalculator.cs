using UnityEngine;

public static class BankCalculator
{
    public static float GetDepositRate(int years)
    {
        switch (years)
        {
            case 1: return 0.14f;
            case 2: return 0.13f;
            case 3: return 0.12f;
            case 4: return 0.11f;
            case 5: return 0.10f;
            default: return 0.10f;
        }
    }

    public static float GetCreditRate(int years)
    {
        switch (years)
        {
            case 1: return 0.18f;
            case 2: return 0.19f;
            case 3: return 0.20f;
            case 4: return 0.21f;
            case 5: return 0.22f;
            default: return 0.22f;
        }
    }

    public static int CalculateDepositFinalAmount(int amount, int years)
    {
        float rate = GetDepositRate(years);
        return Mathf.RoundToInt(amount * Mathf.Pow(1f + rate, years));
    }

    public static int CalculateCreditFinalPayment(int amount, int years)
    {
        float rate = GetCreditRate(years);

        return Mathf.RoundToInt(
            amount * Mathf.Pow(1f + rate, years)
        );
    }
}