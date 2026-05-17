using UnityEngine;
using System.Collections.Generic;


public enum DifficultyLevel { Easy, Normal, Hard }

[CreateAssetMenu(fileName = "Competitor", menuName = "Kursovaya/Competitor AI")]
public class CompetitorAI : ScriptableObject
{
    public string competitorName = "Теневой Конкурент";
    public Sprite portrait;

    [Header("Статистика")]
    public int balance       = 50000;
    public int passiveIncome = 800;



    [Header("Пулы событий (назначить из Assets/Random Events)")]
    [Tooltip("Luck — «лёгкие» события для помощи игроку")]
    public List<RandomEvent> luckEvents = new List<RandomEvent>();

    [Tooltip("UltraLuck — крупные события для помощи игроку")]
    public List<RandomEvent> ultraLuckEvents = new List<RandomEvent>();

    [Tooltip("Unluck — стандартные негативные события")]
    public List<RandomEvent> unluckEvents = new List<RandomEvent>();

    [Tooltip("UltraUnluck — тяжёлые негативные события")]
    public List<RandomEvent> ultraUnluckEvents = new List<RandomEvent>();



    // Шанс того, что саботаж будет использовать UltraUnluck вместо Unluck
    private float UltraChance(DifficultyLevel d) => d switch
    {
        DifficultyLevel.Easy   => 0.05f,
        DifficultyLevel.Normal => 0.20f,
        DifficultyLevel.Hard   => 0.45f,
        _                      => 0.20f
    };

    // Общий максимальный шанс саботажа
    private float MaxSabotageChance(DifficultyLevel d) => d switch
    {
        DifficultyLevel.Easy   => 0.20f,
        DifficultyLevel.Normal => 0.40f,
        DifficultyLevel.Hard   => 0.65f,
        _                      => 0.40f
    };

    // Шанс помочь игроку при низкой агрессии
    private float HelpChance(DifficultyLevel d) => d switch
    {
        DifficultyLevel.Easy   => 0.55f,
        DifficultyLevel.Normal => 0.35f,
        DifficultyLevel.Hard   => 0.15f,
        _                      => 0.35f
    };

    // Максимальный прирост пассивного дохода конкурента за ход
    private int IncomeGrowthMax(DifficultyLevel d) => d switch
    {
        DifficultyLevel.Easy   => 200,
        DifficultyLevel.Normal => 400,
        DifficultyLevel.Hard   => 700,
        _                      => 400
    };


    private const float AggressionThresholdHelp  = 0.25f;
    private const float AggressionThresholdLight = 0.45f;
    private const float AggressionThresholdHeavy = 0.70f;

    private const float WeightBalance = 0.60f;
    private const float WeightAssets  = 0.40f;

    private const int AssetsConsideredMany = 4;
    private const int MaxPassiveIncome     = 5000;



    private const int BonusMin = -20;
    private const int BonusMax =  30;


    public void SimulateTurn(PlayerStats player, DifficultyLevel difficulty)
    {
        // Конкурент растёт сам по себе
        balance += passiveIncome;
        int growth = Random.Range(100, IncomeGrowthMax(difficulty));
        passiveIncome = Mathf.Min(passiveIncome + growth, MaxPassiveIncome);

        // Иногда доход конкурента немного падает (форс-мажор)
        if (Random.value < 0.15f)
            passiveIncome = Mathf.Max(Mathf.RoundToInt(passiveIncome * 0.9f), 500);

        float aggression = CalculateAggression(player);
        LogAggression(aggression, difficulty);

        if (aggression < AggressionThresholdHelp)
        {
            // Конкурент «помогает» (рыночная конъюнктура) — применяем Luck-событие
            if (Random.value < HelpChance(difficulty))
                TriggerHelpEvent(player, difficulty);
        }
        else
        {
            float sabotageChance = Mathf.Lerp(
                0.08f,
                MaxSabotageChance(difficulty),
                Mathf.InverseLerp(AggressionThresholdHelp, 1.0f, aggression));

            if (Random.value < sabotageChance)
                TriggerSabotageEvent(player, aggression, difficulty);
        }
    }



    private void TriggerHelpEvent(PlayerStats player, DifficultyLevel difficulty)
    {
        // На Easy иногда достаётся даже UltraLuck
        bool useUltra = (difficulty == DifficultyLevel.Easy) && (Random.value < 0.25f);
        List<RandomEvent> pool = (useUltra && ultraLuckEvents.Count > 0)
            ? ultraLuckEvents
            : luckEvents;

        RandomEvent ev = PickEventFromPool(pool);
        if (ev == null)
        {
            LogEvent($"[{competitorName}] Пул Luck пустой — помощь пропущена.");
            return;
        }

        if (!IsEventApplicable(ev, player))
        {
            LogEvent($"[{competitorName}] Событие '{ev.title}' неприменимо — пропуск.");
            return;
        }

        ApplyEventToPlayer(ev, player, isSabotage: false);
        ShowNotification(ev.title, ev.description, false);
        LogEvent($"<color=green>[{competitorName}] Помощь: {ev.title}</color>");
    }

  

    private void TriggerSabotageEvent(PlayerStats player, float aggression, DifficultyLevel difficulty)
    {
        // При лёгкой агрессии — только Unluck, никогда Ultra
        bool canUseUltra = aggression >= AggressionThresholdHeavy;
        bool useUltra    = canUseUltra && (Random.value < UltraChance(difficulty));

        List<RandomEvent> pool = (useUltra && ultraUnluckEvents.Count > 0)
            ? ultraUnluckEvents
            : (unluckEvents.Count > 0 ? unluckEvents : ultraUnluckEvents);

        List<RandomEvent> applicable = FilterApplicable(pool, player);
        if (applicable.Count == 0)
        {
            LogEvent($"[{competitorName}] Нет применимых событий для саботажа.");
            return;
        }

        RandomEvent ev = PickWeightedEvent(applicable);
        if (ev == null) return;

        ApplyEventToPlayer(ev, player, isSabotage: true);
        ShowNotification(ev.title, ev.description, true);
        LogSabotage(ev.title);
    }

  

    private void ApplyEventToPlayer(RandomEvent ev, PlayerStats player, bool isSabotage)
    {
        switch (ev.eventType)
        {
            // 0 — фиксированное изменение баланса
            case 0:
                int amount0 = isSabotage ? -Mathf.Abs(ev.balanceAmount) : ev.balanceAmount;
                player.Balance += amount0;
                player.CheckDebtState();
                player.UpdateUI();
                break;

            // 1 — изменение настроения
            case 1:
                int moodDelta = isSabotage ? -Mathf.Abs(ev.moodAmount) : ev.moodAmount;
                player.Mood = Mathf.Clamp(player.Mood + moodDelta, 0, 100);
                player.UpdateUI();
                break;

            // 2 — изменение % зарплаты
            case 2:
                int salaryPct = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.jobIncomePercentBonus = Mathf.Clamp(
                    player.jobIncomePercentBonus + salaryPct, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            // 3 — изменение % дохода бизнеса
            case 3:
                int bizPct = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.businessIncomePercentBonus = Mathf.Clamp(
                    player.businessIncomePercentBonus + bizPct, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            // 4 — изменение % дохода недвижимости
            case 4:
                int realtyPct = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.realtyIncomePercentBonus = Mathf.Clamp(
                    player.realtyIncomePercentBonus + realtyPct, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            // 5 — штраф процентом от баланса (потерял бумажник)
            case 5:
                int loss5 = Mathf.RoundToInt(player.Balance * ev.percentAmount * 0.01f);
                player.Balance -= Mathf.Abs(loss5);
                player.CheckDebtState();
                player.UpdateUI();
                break;

            // 6 — штраф баланса + временной штраф (болезнь)

            case 6:
                player.Balance -= Mathf.Abs(ev.balanceAmount);
                player.CheckDebtState();
                if (ev.timePenalty > 0 && ev.durationPeriods > 0)
                    player.ApplyTimePenalty(ev.timePenalty, ev.durationPeriods);
                player.UpdateUI();
                break;

            // 7 — добавить объект недвижимости (наследство)
            //     При саботаже — события типа 7 не используются (см. IsEventApplicable)
            case 7:
                if (!isSabotage && ev.realtyToAdd != null)
                {
                    player.AddRealty(ev.realtyToAdd);
                    player.RecalculateIncomePublic();
                    player.UpdateUI();
                }
                break;

            // 8 — банковский бонус/штраф (удвоение / полудел дохода)
            case 8:
                int incomeBonus = isSabotage ? -10 : 10;
                player.jobIncomePercentBonus = Mathf.Clamp(
                    player.jobIncomePercentBonus + incomeBonus, BonusMin, BonusMax);
                player.businessIncomePercentBonus = Mathf.Clamp(
                    player.businessIncomePercentBonus + incomeBonus, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            // 9 — банкротство банка: вклады выше 1.4 млн сгорают
            case 9:
                if (isSabotage)
                {
                    const int InsuranceThreshold = 1_400_000;
                    if (player.Balance > InsuranceThreshold)
                        player.Balance = InsuranceThreshold;
                    player.CheckDebtState();
                    player.UpdateUI();
                }
                break;

            // 10 — пожар: уничтожает случайный объект недвижимости
            case 10:
                if (isSabotage)
                {
                    player.RemoveRandomRealty();
                    player.RecalculateIncomePublic();
                    player.UpdateUI();
                }
                break;

            default:
                LogEvent($"[{competitorName}] Неизвестный eventType={ev.eventType}, " +
                         $"событие '{ev.title}' пропущено.");
                break;
        }
    }



    private bool IsEventApplicable(RandomEvent ev, PlayerStats player)
    {
        switch (ev.eventType)
        {
            case 3:  return HasBusiness(player);
            case 4:  return HasRealty(player);
            case 7:  return ev.realtyToAdd != null;
            case 10: return HasRealty(player);
            default: return true;
        }
    }

    private List<RandomEvent> FilterApplicable(List<RandomEvent> pool, PlayerStats player)
    {
        var result = new List<RandomEvent>();
        foreach (var ev in pool)
            if (IsEventApplicable(ev, player))
                result.Add(ev);
        return result;
    }


    private RandomEvent PickEventFromPool(List<RandomEvent> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        return PickWeightedEvent(pool);
    }

    private RandomEvent PickWeightedEvent(List<RandomEvent> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        int totalWeight = 0;
        foreach (var ev in pool)
            totalWeight += Mathf.Max(1, ev.probability);

        int roll       = Random.Range(0, totalWeight);
        int cumulative = 0;
        foreach (var ev in pool)
        {
            cumulative += Mathf.Max(1, ev.probability);
            if (roll < cumulative) return ev;
        }
        return pool[pool.Count - 1];
    }



    private float CalculateAggression(PlayerStats player)
    {
        float balanceFactor;
        if (balance <= 0)
            balanceFactor = 1.0f;
        else if (player.Balance <= 0)
            balanceFactor = 0.30f;
        else
        {
            float total = (float)player.Balance + balance;
            balanceFactor = Mathf.Clamp01((float)player.Balance / total);
        }

        int assetCount  = CountPlayerAssets(player);
        float assetFactor = Mathf.Clamp01((float)assetCount / AssetsConsideredMany);

        return Mathf.Clamp01(balanceFactor * WeightBalance + assetFactor * WeightAssets);
    }



    private int CountPlayerAssets(PlayerStats player)
    {
        int count = 0;
        foreach (var job in player.activeJobs)
            if (job.isBusiness || job.isRealty) count++;
        return count;
    }

    private bool HasBusiness(PlayerStats player)
    {
        foreach (var job in player.activeJobs)
            if (job.isBusiness) return true;
        return false;
    }

    private bool HasRealty(PlayerStats player)
    {
        foreach (var job in player.activeJobs)
            if (job.isRealty) return true;
        return false;
    }



    private void ShowNotification(string title, string description, bool isSabotage)
    {
        UIManager ui = Object.FindObjectOfType<UIManager>();
        if (ui != null)
            ui.ShowCompetitorEvent(competitorName, $"{title}: {description}", isSabotage);
    }



    public string GetLeaderText(PlayerStats player)
    {
        if (balance > player.Balance * 1.3f)
            return $"<color=red>{competitorName} лидирует!</color>";
        else if (balance > player.Balance)
            return $"{competitorName} немного впереди";
        else
            return $"Вы опережаете {competitorName}";
    }



    private void LogSabotage(string message)
    {
#if UNITY_EDITOR
        Debug.Log($"<color=red>[{competitorName}] Саботаж: {message}</color>");
#endif
    }

    private void LogEvent(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#endif
    }

    private void LogAggression(float aggression, DifficultyLevel difficulty)
    {
#if UNITY_EDITOR
        string level = aggression < AggressionThresholdHelp  ? "НИЗКАЯ (помощь)"  :
                       aggression < AggressionThresholdLight ? "ЛЁГКАЯ"           :
                       aggression < AggressionThresholdHeavy ? "СРЕДНЯЯ"          : "ВЫСОКАЯ";
        Debug.Log($"[{competitorName}] Сложность: {difficulty} | Агрессия: {aggression:F2} → {level}");
#endif
    }
}
