using UnityEngine;

public enum OpportunityType
{
    Job,
    Business,
    Invest,
    Realty
}


[CreateAssetMenu(fileName = "New Opportunity", menuName = "Game/Opportunity")]


public class OpportunityData : ScriptableObject
{
    public string title;
    public string description;
    public OpportunityType type;
    public SkillData requiredSkill;

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
    public int investCost;
    public int investRisk; // 1-5

    // --- Недвижимость ---
    public int realtyCost;
    public int realtyIncome;
    public int realtyTimeCost;
}