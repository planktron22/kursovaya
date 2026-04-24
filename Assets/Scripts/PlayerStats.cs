using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int Balance;
    public int Income;
    public int Loss;
    public int FreeTime;
    public int Health;
    public int Mood;

    public int NetIncome => Income - Loss;

    private PlayerStatsInfo statsUI;

    void Start()
    {
        statsUI = FindObjectOfType<PlayerStatsInfo>();
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (statsUI != null)
        {
            statsUI.UpdateStats(this);
        }
    }

    // --- Методы изменения параметров ---

    public void AddBalance(int value)
    {
        Balance += value;
        UpdateUI();
    }

    public void TakeBalance(int value)
    {
        Balance -= value;
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
}