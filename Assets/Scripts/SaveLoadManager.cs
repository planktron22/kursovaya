using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    public static bool loadGameOnStart = false;

    public static string SavePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    public static void SaveGame(PlayerStats player)
    {
        SaveData data = new SaveData();

        data.balance = player.Balance;
        data.mood = player.Mood;
        data.freeTime = player.FreeTime;
        data.age = player.Age;

        data.jobIncomePercentBonus = player.jobIncomePercentBonus;
        data.businessIncomePercentBonus = player.businessIncomePercentBonus;
        data.realtyIncomePercentBonus = player.realtyIncomePercentBonus;

        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            data.currentTile = movement.currentTile;
        }

        foreach (var active in player.activeJobs)
        {
            data.activeAssets.Add(new SavedActive
            {
                opportunityId = active.jobData.id,
                workedPeriods = active.workedPeriods,
                currentDelay = active.currentDelay
            });
        }

        foreach (var invest in player.activeInvests)
        {
            data.invests.Add(new SavedInvest
            {
                opportunityId = invest.investData.id,
                currentCost = invest.currentCost,
                amount = invest.amount
            });
        }

        foreach (var skill in player.learnedSkills)
        {
            data.learnedSkillIds.Add(skill.id);
        }

        foreach (var study in player.studyingSkills)
        {
            data.studyingSkills.Add(new SavedSkillStudy
            {
                skillId = study.skill.id,
                remainingPeriods = study.remainingPeriods,
                timeCost = study.timeCost
            });
        }

        foreach (var person in player.knownPeople)
        {
            data.knownPersonIds.Add(person.id);
        }

        foreach (var deal in player.activeBankDeals)
        {
            data.bankDeals.Add(new SavedBankDeal
            {
                isDeposit = deal.isDeposit,
                amount = deal.amount,
                years = deal.years,
                remainingPeriods = deal.remainingPeriods,
                finalAmount = deal.finalAmount
            });
        }

        if (AchievementManager.Instance != null)
        {
            data.unlockedAchievementIds = AchievementManager.Instance.GetUnlockedAchievementIds();
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Игра сохранена: " + SavePath);
    }

    public static SaveData LoadSaveFile()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("Файл сохранения не найден");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void LoadGameFromMenu()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("Сохранение не найдено");
            return;
        }

        loadGameOnStart = true;
        SceneManager.LoadScene("SampleScene");
    }
}