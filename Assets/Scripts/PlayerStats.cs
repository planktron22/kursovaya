using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int Balance;

    public int Income;
    public int Loss;
    public int FreeTime;
    public int Health;
    public int Mood;
    public int Age;

    public int NetIncome => Income - Loss;

    private PlayerStatsInfo statsUI;

    void Start()
    {
        statsUI = FindObjectOfType<PlayerStatsInfo>();

        ApplyDifficulty();
        UpdateUI();
    }

    void ApplyDifficulty()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 0);

        switch (difficulty)
        {
            case 0:
                Balance = 500000;
                Income = 80000;
                Loss = 55000;
                FreeTime = 100;
                Age = 40;
                break;

            case 1:
                Balance = 200000;
                Income = 50000;
                Loss = 45000;
                FreeTime = 70;
                Age = 30;
                break;

            case 2:
                Balance = 150000;
                Income = 40000;
                Loss = 50000;
                FreeTime = 40;
                Age = 25;
                break;
        }

        Health = 100;
        Mood = 100;
    }

    public void UpdateUI()
    {
        if (statsUI != null)
        {
            statsUI.UpdateStats(this);
        }
    }

    // --- изменения параметров ---

    public void ChangeBalance(int value)
    {
        Balance += value;
        UpdateUI();
    }

    public void AddIncome(int value)
    {
        Income += value;
        UpdateUI();
    }

    public void AddLoss(int value)
    {
        Loss += value;
        UpdateUI();
    }

    public void ChangeFreeTime(int value)
    {
        FreeTime += value;
        UpdateUI();
    }

    public void ChangeHealth(int value)
    {
        Health += value;
        UpdateUI();
    }

    public void ChangeMood(int value)
    {
        Mood += value;
        UpdateUI();
    }

    // период
    public void ApplyPeriod()
    {
        Balance += NetIncome;
        UpdateUI();
    }

    //  год 
    public void DecreaseAge()
    {
        Age -= 1;

        Debug.Log("Прошел год. Осталось лет: " + Age);

        UpdateUI();
    }
}