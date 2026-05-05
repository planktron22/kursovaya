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


    public int TotalIncome;
    public int TotalLoss;

    public int NetIncome => TotalIncome - TotalLoss;


    public List<PlayerActive> activeJobs = new List<PlayerActive>();

    public List<PlayerInvest> activeInvests = new List<PlayerInvest>();

    private OpportunityDatabase opportunityDatabase;


    private PlayerStatsInfo statsUI;
    private JobListUI jobListUI;

    void Start()
    {
        statsUI = FindObjectOfType<PlayerStatsInfo>();
        jobListUI = FindObjectOfType<JobListUI>();
        opportunityDatabase = FindObjectOfType<OpportunityDatabase>();

        ApplyDifficulty();

        InitializeInvests(); 

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
                Balance = 10000000;   // тестовая
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
            if (job.isBusiness)
            {
                // если бизнес еще не запустился
                if (job.currentDelay < job.startDelay)
                    continue;

                TotalIncome += job.jobData.businessIncome;
            }
            else
            {
                TotalIncome += CalculateJobIncome(job.jobData);
            }

            if (job.isRealty)
            {
                TotalIncome += job.jobData.realtyIncome;
            }
        }

        TotalLoss = Loss;
    }

    // --- ПЕРИОД ---
    public void ApplyPeriod()
    {
        RecalculateIncome();

        UpdateInvestPrices();

        Balance += NetIncome;
        foreach (var job in activeJobs)
        {
            job.workedPeriods++;

            if (job.isBusiness)
            {
                job.currentDelay++;
            }
        }

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
        {
            Debug.Log($"Недостаточно свободного времени! Нужно: {job.jobHours}, доступно: {FreeTime}");
            return;
        }

        activeJobs.Add(new PlayerActive
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

    public bool HasJob(OpportunityData job)
    {
        foreach (var j in activeJobs)
        {
            if (j.jobData == job)
                return true;
        }
        return false;
    }

    public PlayerActive GetJob(OpportunityData data)
    {
        foreach (var active in activeJobs)
        {
            if (active.jobData == data)
                return active;
        }

        return null;
    }

    // --- УВОЛЬНЕНИЕ  ---
    public void RemoveJob(PlayerActive job)
    {
        if (activeJobs.Contains(job))
        {
            activeJobs.Remove(job);
            FreeTime += job.timeCost;

            RecalculateIncome();
            UpdateUI();
        }
    }

    public void ApplyBusiness(OpportunityData data)
    {
        // проверка денег
        if (Balance < data.businessCost)
        {
            Debug.Log($"❌ Недостаточно денег! Нужно: {data.businessCost}, есть: {Balance}");
            return;
        }

        // проверка времени
        if (FreeTime < data.businessTimeCost)
        {
            Debug.Log($"❌ Недостаточно времени!");
            return;
        }

        // проверка на дубликат
        foreach (var b in activeJobs)
        {
            if (b.jobData == data)
                return;
        }

        // списываем ресурсы
        Balance -= data.businessCost;
        FreeTime -= data.businessTimeCost;

        activeJobs.Add(new PlayerActive
        {
            title = data.title,
            timeCost = data.businessTimeCost,
            jobData = data,

            isBusiness = true,
            startDelay = data.businessStartTime,
            currentDelay = 0
        });

        Debug.Log($"✅ Вы открыли бизнес: {data.title}");

        RecalculateIncome();
        UpdateUI();
    }

    public void SellBusiness(PlayerActive business)
    {
        if (!business.isBusiness)
            return;

        if (business.currentDelay < business.startDelay)
        {
            Debug.Log("Бизнес можно продать только после запуска");
            return;
        }

        Balance += business.jobData.businessCost;
        FreeTime += business.timeCost;

        activeJobs.Remove(business);

        RecalculateIncome();
        UpdateUI();

        Debug.Log("Бизнес продан: " + business.title);
    }

    public void ApplyRealty(OpportunityData data)
    {
        if (Balance < data.realtyCost)
        {
            Debug.Log($"Недостаточно денег! Нужно: {data.realtyCost}, есть: {Balance}");
            return;
        }

        if (FreeTime < data.realtyTimeCost)
        {
            Debug.Log($"Недостаточно времени! Нужно: {data.realtyTimeCost}, есть: {FreeTime}");
            return;
        }

        foreach (var active in activeJobs)
        {
            if (active.jobData == data)
            {
                Debug.Log("Эта недвижимость уже куплена");
                return;
            }
        }

        Balance -= data.realtyCost;
        FreeTime -= data.realtyTimeCost;

        activeJobs.Add(new PlayerActive
        {
            title = data.title,
            timeCost = data.realtyTimeCost,
            jobData = data,
            isRealty = true
        });

        RecalculateIncome();
        UpdateUI();
    }

    public void SellRealty(PlayerActive realty)
    {
        if (!realty.isRealty)
            return;

        Balance += realty.jobData.realtyCost;
        FreeTime += realty.timeCost;

        activeJobs.Remove(realty);

        RecalculateIncome();
        UpdateUI();

        Debug.Log("Недвижимость продана: " + realty.title);
    }

    void InitializeInvests()
    {
        if (opportunityDatabase == null)
            return;

        activeInvests.Clear();

        foreach (var invest in opportunityDatabase.invests)
        {
            activeInvests.Add(new PlayerInvest
            {
                title = invest.title,
                investData = invest,
                currentCost = invest.investCost,
                amount = 0
            });
        }
    }

    public PlayerInvest GetInvest(OpportunityData data)
    {
        foreach (var invest in activeInvests)
        {
            if (invest.investData == data)
                return invest;
        }

        return null;
    }

    public void BuyInvest(OpportunityData data, int amount)
    {
        PlayerInvest invest = GetInvest(data);

        if (invest == null)
            return;

        if (amount <= 0)
        {
            Debug.Log("Количество акций должно быть больше 0");
            return;
        }

        if (invest.amount + amount > 1000)
        {
            Debug.Log("Нельзя владеть больше чем 1000 акциями");
            return;
        }

        int totalCost = invest.currentCost * amount;

        if (Balance < totalCost)
        {
            Debug.Log($"Недостаточно денег! Нужно: {totalCost}, есть: {Balance}");
            return;
        }

        Balance -= totalCost;
        invest.amount += amount;

        Debug.Log($"Куплено акций {invest.title}: {amount}");

        UpdateUI();
    }

    public void SellInvest(OpportunityData data, int amount)
    {
        PlayerInvest invest = GetInvest(data);

        if (invest == null)
            return;

        if (amount <= 0)
        {
            Debug.Log("Количество акций должно быть больше 0");
            return;
        }

        if (invest.amount < amount)
        {
            Debug.Log($"Недостаточно акций для продажи. Есть: {invest.amount}");
            return;
        }

        invest.amount -= amount;
        Balance += invest.currentCost * amount;

        Debug.Log($"Продано акций {invest.title}: {amount}");

        UpdateUI();
    }

    void UpdateInvestPrices()
    {
        foreach (var invest in activeInvests)
        {
            int risk = invest.investData.investRisk;

            int growChance = 90;
            float maxChangePercent = 1f;

            switch (risk)
            {
                case 1:
                    growChance = 90;
                    maxChangePercent = 1f;
                    break;

                case 2:
                    growChance = 75;
                    maxChangePercent = 3f;
                    break;

                case 3:
                    growChance = 60;
                    maxChangePercent = 6f;
                    break;

                case 4:
                    growChance = 50;
                    maxChangePercent = 9f;
                    break;

                case 5:
                    growChance = 30;
                    maxChangePercent = 15f;
                    break;
            }

            bool grows = Random.Range(1, 101) <= growChance;

            float percent = Random.Range(0.1f, maxChangePercent);
            float multiplier = percent / 100f;

            int change = Mathf.RoundToInt(invest.currentCost * multiplier);

            if (change < 1)
                change = 1;

            if (grows)
                invest.currentCost += change;
            else
                invest.currentCost -= change;

            if (invest.currentCost < 1)
                invest.currentCost = 1;

            Debug.Log($"{invest.title}: новая цена {invest.currentCost}");
        }
    }
}