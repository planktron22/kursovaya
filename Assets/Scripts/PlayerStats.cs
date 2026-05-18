using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    // --- ОСНОВНЫЕ ---
    public int Balance;
    public int Loss;
    public int FreeTime;
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
    public List<PersonData> knownPeople = new List<PersonData>();

    [Header("Communication Skills")]
    public SkillData communicationLevel1;
    public SkillData communicationLevel2;
    public SkillData communicationLevel3;

    public List<PlayerBankDeal> activeBankDeals = new List<PlayerBankDeal>();

    [SerializeField] private CompetitorAI competitor;

    private PlayerStatsInfo statsUI;
    private JobListUI jobListUI;

    private PeriodPopupSpawner popupSpawner;

    void Start()
    {
        statsUI = FindObjectOfType<PlayerStatsInfo>();
        jobListUI = FindObjectOfType<JobListUI>();
        opportunityDatabase = FindObjectOfType<OpportunityDatabase>();

        if (SaveLoadManager.loadGameOnStart)
        {
            SaveLoadManager.loadGameOnStart = false;
            LoadFromSave();
        }
        else
        {
            ApplyDifficulty();
            InitializeInvests();
        }

        popupSpawner = GetComponent<PeriodPopupSpawner>();

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
                if (job.currentDelay < job.startDelay)
                    continue;

                TotalIncome += ApplyPercentBonus(
                    job.jobData.businessIncome,
                    businessIncomePercentBonus
                );
            }
            else if (job.isRealty)
            {
                TotalIncome += ApplyPercentBonus(
                    job.jobData.realtyIncome,
                    realtyIncomePercentBonus
                );
            }
            else
            {
                TotalIncome += CalculateJobIncome(job.jobData);
            }
        }

        TotalLoss = rentLoss + foodLoss + TaxLoss;
    }

    public void RecalculateIncomePublic()
    {
        RecalculateIncome();
        UpdateUI();
    }

    // --- ПЕРИОД ---
    public void ApplyPeriod()
    {     
        Balance += NetIncome;
        RecalculateIncome();
        
        competitor?.SimulateTurn(this);

        if (popupSpawner != null)
        {
            popupSpawner.ShowPeriodPopup(NetIncome);
        }


        UpdateInvestPrices();

        UpdateStudyingSkills();

        UpdateBankDeals();

        ApplyMoodChanges();
        UpdateDepression();

        CheckDebtState();

        UpdateTemporaryEffects();


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

        return ApplyPercentBonus(baseIncome + bonusIncome, jobIncomePercentBonus);
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

        if (job.requiredPerson != null && !KnowsPerson(job.requiredPerson))
        {
            Debug.Log("Нужно знакомство: " + job.requiredPerson.personName);
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

        if (data.requiredPerson != null && !KnowsPerson(data.requiredPerson))
        {
            Debug.Log("Нужно знакомство: " + data.requiredPerson.personName);
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
        CheckDebtState();
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

        if (data.requiredPerson != null && !KnowsPerson(data.requiredPerson))
        {
            Debug.Log("Нужно знакомство: " + data.requiredPerson.personName);
            return;
        }

        if (data.requiredSkill != null && !HasSkill(data.requiredSkill))
        {
            Debug.Log("Не хватает навыка: " + data.requiredSkill.title);
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
        CheckDebtState();
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

    public void ApplyTimePenalty(int timePenalty, int durationPeriods)
{
    FreeTime -= timePenalty;
 
    temporaryEffects.Add(new TemporaryEffect
    {
        title            = "Временный штраф",
        remainingPeriods = durationPeriods,
        timePenalty      = timePenalty
    });
 
    Debug.Log($"Применён временный штраф: -{timePenalty} ч. на {durationPeriods} периодов");
 
    UpdateUI();
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
        CheckDebtState();

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

        if (skill.requiredSkill != null && !HasSkill(skill.requiredSkill))
        {
            Debug.Log("Сначала нужно изучить навык: " + skill.requiredSkill.title);
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

        CheckDebtState();
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

        CheckDebtState();
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

    public bool KnowsPerson(PersonData person)
    {
        if (person == null)
            return true;

        return knownPeople.Contains(person);
    }

    public void TryMeetRandomPerson()
    {
        if (opportunityDatabase == null)
        {
            Debug.LogError("OpportunityDatabase не найден");
            return;
        }

        if (opportunityDatabase.people == null || opportunityDatabase.people.Length == 0)
        {
            Debug.Log("В базе нет людей для знакомства");
            return;
        }

        if (FreeTime < 5)
        {
            Debug.Log("Недостаточно свободного времени для знакомства. Нужно 5 ч.");
            return;
        }

        FreeTime -= 5;

        PersonData person = opportunityDatabase.people[
            Random.Range(0, opportunityDatabase.people.Length)
        ];

        if (knownPeople.Contains(person))
        {
            Debug.Log("Вы уже знакомы с: " + person.personName);
            UpdateUI();
            return;
        }

        int level = GetCommunicationLevel();
        int chance = GetCommunicationChance();

        Debug.Log("Уровень общения: " + level);
        Debug.Log("Шанс знакомства: " + chance + "%");

        int roll = Random.Range(1, 101);

        Debug.Log(
            $"Попытка знакомства: {person.personName}. " +
            $"Шанс: {chance}%, выпало: {roll}"
        );

        if (roll <= chance)
        {
            knownPeople.Add(person);

            Debug.Log("Успешное знакомство: " + person.personName);
        }
        else
        {
            Debug.Log("Знакомство не удалось: " + person.personName);
        }

        UpdateUI();
    }

    int GetCommunicationLevel()
    {
        if (communicationLevel3 != null && learnedSkills.Contains(communicationLevel3))
            return 3;

        if (communicationLevel2 != null && learnedSkills.Contains(communicationLevel2))
            return 2;

        if (communicationLevel1 != null && learnedSkills.Contains(communicationLevel1))
            return 1;

        return 0;
    }

    int GetCommunicationChance()
    {
        int level = GetCommunicationLevel();

        switch (level)
        {
            case 1: return 45;
            case 2: return 65;
            case 3: return 90;
            default: return 20;
        }
    }

    [Header("Mood System")]
    public bool isDepressed = false;
    public int moodZeroPeriods = 0;
    public int depressionPeriodsLeft = 0;

    public int depressionTreatmentCost = 500000;
    public int depressionTimeCost = 100;
    public int depressionDuration = 6;

    void ApplyMoodChanges()
    {
        int moodChange = 0;

        // --- свободное время ---
        if (FreeTime < 10)
        {
            moodChange -= 10;
        }
        else if (FreeTime < 25)
        {
            moodChange -= 5;
        }
        else if (FreeTime > 150)
        {
            moodChange += 10;
        }
        else if (FreeTime > 100)
        {
            moodChange += 5;
        }

        // --- чистая прибыль ---
        if (NetIncome < 0)
        {
            int lossPerPeriod = Mathf.Abs(NetIncome);

            if (lossPerPeriod > 0 && Balance / lossPerPeriod <= 3)
            {
                moodChange -= 10;
            }
            else
            {
                moodChange -= 5;
            }
        }
        else if (NetIncome > 0)
        {
            if (TotalLoss > 0 && TotalIncome >= TotalLoss * 2)
            {
                moodChange += 10;
            }
            else
            {
                moodChange += 5;
            }
        }

        Mood += moodChange;
        Mood = Mathf.Clamp(Mood, 0, 100);

        Debug.Log("Изменение настроения за период: " + moodChange + ". Настроение: " + Mood);
    }

    void UpdateDepression()
    {
        if (Mood <= 0)
        {
            moodZeroPeriods++;
        }
        else
        {
            moodZeroPeriods = 0;
        }

        if (!isDepressed && moodZeroPeriods >= 2)
        {
            StartDepressionTreatment();
        }

        if (isDepressed)
        {
            depressionPeriodsLeft--;

            if (depressionPeriodsLeft <= 0)
            {
                EndDepressionTreatment();
            }
        }
    }

    void StartDepressionTreatment()
    {
        isDepressed = true;
        depressionPeriodsLeft = depressionDuration;

        Balance -= depressionTreatmentCost;
        FreeTime -= depressionTimeCost;

        Debug.Log(
            "Игрок впал в депрессию. Начат курс лечения: -" +
            depressionTreatmentCost + " р., -" +
            depressionTimeCost + " ч. на " +
            depressionDuration + " периодов"
        );
    }

    void EndDepressionTreatment()
    {
        isDepressed = false;
        moodZeroPeriods = 0;

        FreeTime += depressionTimeCost;
        Mood = 50;

        Debug.Log("Курс лечения завершён. Настроение восстановлено до 50");
    }

    [Header("Debt / Failure")]
    public bool debtMode = false;

    public void CheckDebtState()
    {
        if (Balance >= 0)
        {
            debtMode = false;
            return;
        }

        debtMode = true;

        Debug.Log("Баланс ниже нуля. Проверка активов...");

        if (HasSellableAssets())
        {
            Debug.Log("Есть активы для продажи. Игрок может погасить долг вручную.");
            return;
        }

        AutoCloseDepositsOrGameOver();
    }

    bool HasSellableAssets()
    {
        foreach (var asset in activeJobs)
        {
            if (asset.isBusiness || asset.isRealty)
                return true;
        }

        foreach (var invest in activeInvests)
        {
            if (invest.amount > 0)
                return true;
        }

        return false;
    }

    void AutoCloseDepositsOrGameOver()
    {
        bool hadDeposit = false;

        for (int i = activeBankDeals.Count - 1; i >= 0; i--)
        {
            PlayerBankDeal deal = activeBankDeals[i];

            if (deal.isDeposit)
            {
                hadDeposit = true;

                Balance += deal.amount;
                activeBankDeals.RemoveAt(i);

                Debug.Log("Вклад закрыт досрочно. Возвращено без процентов: " + deal.amount);
            }
        }

        UpdateUI();

        if (Balance >= 0)
        {
            debtMode = false;
            Debug.Log("Долг погашен за счет вкладов.");
            return;
        }

        if (!hadDeposit)
        {
            Debug.Log("Нет активов и вкладов. Игра окончена.");
        }
        else
        {
            Debug.Log("Вкладов не хватило для погашения долга. Игра окончена.");
        }

        GameOver();
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public List<TemporaryEffect> temporaryEffects = new List<TemporaryEffect>();

    public int jobIncomePercentBonus = 0;
    public int businessIncomePercentBonus = 0;
    public int realtyIncomePercentBonus = 0;

    void UpdateTemporaryEffects()
    {
        for (int i = temporaryEffects.Count - 1; i >= 0; i--)
        {
            temporaryEffects[i].remainingPeriods--;

            if (temporaryEffects[i].remainingPeriods <= 0)
            {
                FreeTime += temporaryEffects[i].timePenalty;

                Debug.Log("Завершился временный эффект: " + temporaryEffects[i].title);

                temporaryEffects.RemoveAt(i);
            }
        }
    }

    public int ApplyPercentBonus(int value, int percent)
    {
        return Mathf.RoundToInt(value * (1f + percent / 100f));
    }

    public void AddRealtyForFree(OpportunityData data)
    {
        if (data == null)
        {
            Debug.Log("Недвижимость не назначена в событии");
            return;
        }

        foreach (var active in activeJobs)
        {
            if (active.jobData == data)
            {
                Debug.Log("Эта недвижимость уже есть");
                return;
            }
        }

        activeJobs.Add(new PlayerActive
        {
            title = data.title,
            timeCost = data.realtyTimeCost,
            jobData = data,
            isRealty = true
        });

        FreeTime -= data.realtyTimeCost;

        RecalculateIncome();
        UpdateUI();

        Debug.Log("Получена недвижимость бесплатно: " + data.title);
    }

    public void BurnLargeDeposits(int limit)
    {
        for (int i = activeBankDeals.Count - 1; i >= 0; i--)
        {
            if (activeBankDeals[i].isDeposit && activeBankDeals[i].amount > limit)
            {
                Debug.Log("Сгорел вклад: " + activeBankDeals[i].amount);
                activeBankDeals.RemoveAt(i);
            }
        }

        UpdateUI();
    }

    public void BurnRandomBusinessOrRealty()
    {
        List<PlayerActive> assets = new List<PlayerActive>();

        foreach (var asset in activeJobs)
        {
            if (asset.isBusiness || asset.isRealty)
            {
                assets.Add(asset);
            }
        }

        if (assets.Count == 0)
        {
            Debug.Log("Нет бизнеса или недвижимости для уничтожения");
            return;
        }

        PlayerActive burned = assets[Random.Range(0, assets.Count)];

        activeJobs.Remove(burned);
        FreeTime += burned.timeCost;

        Debug.Log("Сгорел актив: " + burned.title);

        RecalculateIncome();
        UpdateUI();
    }

    [Header("Expenses")]
    public int rentLoss = 45000;
    public int foodLoss = 30000;
    public int taxPercent = 15;

    public int TaxLoss => Mathf.RoundToInt(TotalIncome * (taxPercent / 100f));

    public void LoadFromSave()
    {
        SaveData data = SaveLoadManager.LoadSaveFile();

        if (data == null)
        {
            ApplyDifficulty();
            InitializeInvests();
            return;
        }

        Balance = data.balance;
        Mood = data.mood;
        FreeTime = data.freeTime;
        Age = data.age;

        jobIncomePercentBonus = data.jobIncomePercentBonus;
        businessIncomePercentBonus = data.businessIncomePercentBonus;
        realtyIncomePercentBonus = data.realtyIncomePercentBonus;

        activeJobs.Clear();
        activeInvests.Clear();
        learnedSkills.Clear();
        studyingSkills.Clear();
        knownPeople.Clear();
        activeBankDeals.Clear();

        PlayerMovement movement = GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.SetCurrentTile(data.currentTile);
        }

        foreach (var saved in data.activeAssets)
        {
            OpportunityData opportunity =
                opportunityDatabase.FindOpportunityById(saved.opportunityId);

            if (opportunity == null)
                continue;

            PlayerActive active = new PlayerActive
            {
                title = opportunity.title,
                timeCost = GetTimeCost(opportunity),
                jobData = opportunity,
                workedPeriods = saved.workedPeriods,
                currentDelay = saved.currentDelay
            };

            if (opportunity.type == OpportunityType.Business)
            {
                active.isBusiness = true;
                active.startDelay = opportunity.businessStartTime;
            }
            else if (opportunity.type == OpportunityType.Realty)
            {
                active.isRealty = true;
            }

            activeJobs.Add(active);
        }

        foreach (var saved in data.invests)
        {
            OpportunityData opportunity =
                opportunityDatabase.FindOpportunityById(saved.opportunityId);

            if (opportunity == null)
                continue;

            activeInvests.Add(new PlayerInvest
            {
                title = opportunity.title,
                investData = opportunity,
                currentCost = saved.currentCost,
                amount = saved.amount
            });
        }

        foreach (string id in data.learnedSkillIds)
        {
            SkillData skill = opportunityDatabase.FindSkillById(id);

            if (skill != null)
                learnedSkills.Add(skill);
        }

        foreach (var saved in data.studyingSkills)
        {
            SkillData skill = opportunityDatabase.FindSkillById(saved.skillId);

            if (skill != null)
            {
                studyingSkills.Add(new PlayerSkillStudy
                {
                    skill = skill,
                    remainingPeriods = saved.remainingPeriods,
                    timeCost = saved.timeCost
                });
            }
        }

        foreach (string id in data.knownPersonIds)
        {
            PersonData person = opportunityDatabase.FindPersonById(id);

            if (person != null)
                knownPeople.Add(person);
        }

        foreach (var saved in data.bankDeals)
        {
            activeBankDeals.Add(new PlayerBankDeal
            {
                isDeposit = saved.isDeposit,
                amount = saved.amount,
                years = saved.years,
                remainingPeriods = saved.remainingPeriods,
                finalAmount = saved.finalAmount
            });
        }

        Debug.Log("Сохранение загружено");
    }

    int GetTimeCost(OpportunityData data)
    {
        switch (data.type)
        {
            case OpportunityType.Job:
                return data.jobHours;

            case OpportunityType.Business:
                return data.businessTimeCost;

            case OpportunityType.Realty:
                return data.realtyTimeCost;

            default:
                return 0;
        }
    }

    public void SaveCurrentGame()
    {
        SaveLoadManager.SaveGame(this);
    }
}