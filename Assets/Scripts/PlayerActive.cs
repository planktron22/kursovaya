using System;

[Serializable]
public class PlayerActive
{
    public string title;
    public int income;
    public int timeCost;
    public OpportunityData jobData;
    public int workedPeriods = 0;

    // --- БИЗНЕС ---
    public bool isBusiness = false;
    public int startDelay;       // сколько ждать
    public int currentDelay = 0; // сколько прошло

    public bool isRealty = false;
}