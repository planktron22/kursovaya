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

    public List<SkillData> learnedSkills = new List<SkillData>();
    public List<PlayerSkillStudy> studyingSkills = new List<PlayerSkillStudy>();

    public List<PlayerBankDeal> activeBankDeals = new List<PlayerBankDeal>();

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

        UpdateStudyingSkills();

        UpdateBankDeals();

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

        if (job.requiredSkill != null && !HasSkill(job.requiredSkill))
        {
            Debug.Log("Не хватает навыка: " + job.requiredSkill.title);
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

        if (data.requiredSkill != null && !HasSkill(data.requiredSkill))
        {
            Debug.Log("Не хватает навыка: " + data.requiredSkill.title);
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
    public bool HasSkill(SkillData skill)
    {
        return learnedSkills.Contains(skill);
    }

    public bool IsStudyingSkill(SkillData skill)
    {
        foreach (var study in studyingSkills)
        {
            if (study.skill == skill)
                return true;
        }

        return false;
    }

    public void StartStudyingSkill(SkillData skill)
    {
        if (HasSkill(skill))
        {
            Debug.Log("Этот навык уже изучен");
            return;
        }

        if (IsStudyingSkill(skill))
        {
            Debug.Log("Этот навык уже изучается");
            return;
        }

        if (Balance < skill.studyCost)
        {
            Debug.Log($"Недостаточно денег. Нужно: {skill.studyCost}, есть: {Balance}");
            return;
        }

        if (FreeTime < skill.studyTimeCost)
        {
            Debug.Log($"Недостаточно свободного времени. Нужно: {skill.studyTimeCost}, есть: {FreeTime}");
            return;
        }

        Balance -= skill.studyCost;
        FreeTime -= skill.studyTimeCost;

        studyingSkills.Add(new PlayerSkillStudy
        {
            skill = skill,
            remainingPeriods = skill.studyPeriods,
            timeCost = skill.studyTimeCost
        });

        Debug.Log("Начато обучение: " + skill.title);

        UpdateUI();
    }

    void UpdateStudyingSkills()
    {
        for (int i = studyingSkills.Count - 1; i >= 0; i--)
        {
            studyingSkills[i].remainingPeriods--;

            if (studyingSkills[i].remainingPeriods <= 0)
            {
                learnedSkills.Add(studyingSkills[i].skill);
                FreeTime += studyingSkills[i].timeCost;

                Debug.Log("Изучен навык: " + studyingSkills[i].skill.title);

                studyingSkills.RemoveAt(i);
            }
        }
    }

    public void OpenDeposit(int amount, int years)
    {
        if (amount <= 0)
        {
            Debug.Log("Введите корректную сумму вклада");
            return;
        }

        if (years < 1 || years > 5)
        {
            Debug.Log("Срок вклада должен быть от 1 до 5 лет");
            return;
        }

        if (Balance < amount)
        {
            Debug.Log($"Недостаточно денег для вклада. Нужно: {amount}, есть: {Balance}");
            return;
        }

        int finalAmount = BankCalculator.CalculateDepositFinalAmount(amount, years);

        Balance -= amount;

        activeBankDeals.Add(new PlayerBankDeal
        {
            isDeposit = true,
            amount = amount,
            years = years,
            remainingPeriods = years * 6,
            finalAmount = finalAmount
        });

        Debug.Log($"Открыт вклад на {years} лет. К получению: {finalAmount}");

        UpdateUI();
    }

    public void OpenCredit(int amount, int years)
    {
        if (amount <= 0)
        {
            Debug.Log("Введите корректную сумму кредита");
            return;
        }

        if (years < 1 || years > 5)
        {
            Debug.Log("Срок кредита должен быть от 1 до 5 лет");
            return;
        }

        int periods = years * 6;
        int hypotheticalIncome = NetIncome * periods;

        int finalPayment = BankCalculator.CalculateCreditFinalPayment(amount, years);

        if (finalPayment > hypotheticalIncome)
        {
            Debug.Log($"Банк отказал в кредите. Итоговый платеж: {finalPayment}, гипотетический доход: {hypotheticalIncome}");
            return;
        }


        Balance += amount;

        activeBankDeals.Add(new PlayerBankDeal
        {
            isDeposit = false,
            amount = amount,
            years = years,
            remainingPeriods = periods,
            finalAmount = finalPayment
        });

        Debug.Log($"Открыт кредит на {years} лет. К выплате: {finalPayment}");

        UpdateUI();
    }

    void UpdateBankDeals()
    {
        for (int i = activeBankDeals.Count - 1; i >= 0; i--)
        {
            activeBankDeals[i].remainingPeriods--;

            if (activeBankDeals[i].remainingPeriods <= 0)
            {
                if (activeBankDeals[i].isDeposit)
                {
                    Balance += activeBankDeals[i].finalAmount;
                    Debug.Log($"Вклад завершён. Получено: {activeBankDeals[i].finalAmount}");
                }
                else
                {
                    Balance -= activeBankDeals[i].finalAmount;
                    Debug.Log($"Кредит завершён. Списано: {activeBankDeals[i].finalAmount}");
                }

                activeBankDeals.RemoveAt(i);
            }
        }
    }
}