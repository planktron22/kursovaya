using System;

[Serializable]
public class PlayerBankDeal
{
    public bool isDeposit; // true = вклад, false = кредит

    public int amount;
    public int years;
    public int remainingPeriods;

    public int finalAmount;
}