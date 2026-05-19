using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int balance;
    public int mood;
    public int freeTime;
    public int age;
    public int currentTile;

    public int jobIncomePercentBonus;
    public int businessIncomePercentBonus;
    public int realtyIncomePercentBonus;

    public List<SavedActive> activeAssets = new List<SavedActive>();
    public List<SavedInvest> invests = new List<SavedInvest>();
    public List<SavedSkillStudy> studyingSkills = new List<SavedSkillStudy>();
    public List<string> learnedSkillIds = new List<string>();
    public List<string> knownPersonIds = new List<string>();
    public List<SavedBankDeal> bankDeals = new List<SavedBankDeal>();

    // Достижения сохраняются вместе с конкретной партией.
    // Новая игра начинает список заново, а загрузка восстанавливает список из save.json.
    public List<string> unlockedAchievementIds = new List<string>();
}

[Serializable]
public class SavedActive
{
    public string opportunityId;
    public int workedPeriods;
    public int currentDelay;
}

[Serializable]
public class SavedInvest
{
    public string opportunityId;
    public int currentCost;
    public int amount;
}

[Serializable]
public class SavedSkillStudy
{
    public string skillId;
    public int remainingPeriods;
    public int timeCost;
}

[Serializable]
public class SavedBankDeal
{
    public bool isDeposit;
    public int amount;
    public int years;
    public int remainingPeriods;
    public int finalAmount;
}
