using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    // --- ОСНОВНЫЕ ---
    public int Balance;
    public int Loss;
    public int FreeTime;
    public int Health;
    public int Mood;
    public int Age;

    // --- ЭКОНОМИКА ---
    public int TotalIncome;
    public int TotalLoss;

    public int NetIncome => TotalIncome - TotalLoss;

    // --- РАБОТЫ ---
    public List<PlayerJob> activeJobs = new List<PlayerJob>();

    // --- UI ---
    private PlayerStatsInfo statsUI;
    private JobListUI jobListUI;

    void Start()
    {
        statsUI = FindObjectOfType<PlayerStatsInfo>();
        jobListUI = FindObjectOfType<JobListUI>();

        ApplyDifficulty();
        RecalculateIncome();
        UpdateUI();
    }

    void ApplyDifficulty()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 0);

        switch (difficulty)
        {
            case 0:
                Balance = 500000;
                Loss = 55000;
                FreeTime = 500;
                Age = 40;
                break;

            case 1:
                Balance = 200000;
                Loss = 45000;
                FreeTime = 500;
                Age = 30;
                break;

            case 2:
                Balance = 150000;
                Loss = 50000;
                FreeTime = 500;
                Age = 25;
                break;
        }

        Health = 100;
        Mood = 100;
    }

    public void UpdateUI()
    {
        if (statsUI != null)
            statsUI.UpdateStats(this);
    }

    // --- ПЕРЕСЧЁТ ДОХОДА ---
    void RecalculateIncome()
    {
        TotalIncome = 0;

        foreach (var job in activeJobs)
        {
            TotalIncome += CalculateJobIncome(job.jobData);
        }

        TotalLoss = Loss;
    }

    // --- ПЕРИОД ---
    public void ApplyPeriod()
    {
        RecalculateIncome();

        Balance += NetIncome;

        Debug.Log($"Период: +{TotalIncome} / -{TotalLoss} / Баланс {Balance}");

        UpdateUI();
    }

    // --- ГОД ---
    public void DecreaseAge()
    {
        Age -= 1;
        UpdateUI();
    }

    // --- РАСЧЁТ ДОХОДА ---
    public int CalculateJobIncome(OpportunityData job)
    {
        int baseIncome = job.jobIncomePerHour * job.jobHours;

        int min = job.jobBonusMin / 50;
        int max = job.jobBonusMax / 50;

        int bonusIncome = 0;

        for (int i = 0; i < job.jobHours; i++)
        {
            int randomStep = Random.Range(min, max + 1) * 50;
            bonusIncome += randomStep;
        }

        return baseIncome + bonusIncome;
    }

    // --- НАЙМ ---
    public void ApplyJob(OpportunityData job)
    {
        foreach (var j in activeJobs)
        {
            if (j.title == job.title)
                return;
        }

        if (FreeTime < job.jobHours)
            return;

        activeJobs.Add(new PlayerJob
        {
            title = job.title,
            timeCost = job.jobHours,
            jobData = job
        });

        FreeTime -= job.jobHours;

        RecalculateIncome();

        UpdateUI();

        if (jobListUI != null)
            jobListUI.GenerateList();
    }

    // --- УВОЛЬНЕНИЕ (позже) ---
    public void RemoveJob(PlayerJob job)
    {
        if (activeJobs.Contains(job))
        {
            activeJobs.Remove(job);
            FreeTime += job.timeCost;

            RecalculateIncome();

            UpdateUI();
        }
    }
}