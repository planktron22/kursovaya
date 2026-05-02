using UnityEngine;

public enum OpportunityType
{
    Job,
    Business,
    Investment,
    RealEstate
}

[CreateAssetMenu(fileName = "New Opportunity", menuName = "Game/Opportunity")]


public class OpportunityData : ScriptableObject
{
    public string title;
    public string description;
    public OpportunityType type;

    // --- Работа ---
    public int jobIncomePerHour;   // доход за час
    public int jobHours;           // количество часов
    public int jobBonusMin;        // мин бонус за час
    public int jobBonusMax;        // макс бонус за час

    // --- Бизнес ---
    public int businessCost;
    public int businessIncome;
    public int businessStartTime;
    public int businessTimeCost;

    // --- Инвестиции ---
    public int investmentRisk; 

    // --- Недвижимость ---
    public int realEstateCost;
    public int realEstateIncome;
    public int realEstateTimeCost;
}