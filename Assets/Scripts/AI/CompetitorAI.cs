using UnityEngine;
using System.Collections.Generic;

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
    public List<RandomEventData> luckEvents = new List<RandomEventData>();

    [Tooltip("UltraLuck — крупные события для помощи игроку")]
    public List<RandomEventData> ultraLuckEvents = new List<RandomEventData>();

    [Tooltip("Unluck — стандартные негативные события")]
    public List<RandomEventData> unluckEvents = new List<RandomEventData>();

    [Tooltip("UltraUnluck — тяжёлые негативные события")]
    public List<RandomEventData> ultraUnluckEvents = new List<RandomEventData>();


    private float UltraChance(int d) => d switch
    {
        0 => 0.05f,   // Easy
        1 => 0.20f,   // Normal/Debug
        2 => 0.45f,   // Hard
        _ => 0.20f
    };

    private float MaxSabotageChance(int d) => d switch
    {
        0 => 0.20f,
        1 => 0.40f,
        2 => 0.65f,
        _ => 0.40f
    };

    private float HelpChance(int d) => d switch
    {
        0 => 0.55f,
        1 => 0.35f,
        2 => 0.15f,
        _ => 0.35f
    };

    private int IncomeGrowthMax(int d) => d switch
    {
        0 => 200,
        1 => 400,
        2 => 700,
        _ => 400
    };


    private const float AggressionThresholdHelp  = 0.25f;
    private const float AggressionThresholdLight = 0.45f;
    private const float AggressionThresholdHeavy = 0.70f;

    private const float WeightBalance = 0.60f;
    private const float WeightAssets  = 0.40f;

    private const int AssetsConsideredMany = 4;
    private const int MaxPassiveIncome     = 5000;

    // Ограничители бонусов игрока
    private const int BonusMin = -20;
    private const int BonusMax =  30;


    public void SimulateTurn(PlayerStats player)
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 0);

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


    private void TriggerHelpEvent(PlayerStats player, int difficulty)
    {
        bool useUltra = (difficulty == 0) && (Random.value < 0.25f);
        List<RandomEventData> pool = (useUltra && ultraLuckEvents.Count > 0)
            ? ultraLuckEvents
            : luckEvents;

        RandomEventData ev = PickEventFromPool(pool);
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



    private void TriggerSabotageEvent(PlayerStats player, float aggression, int difficulty)
    {
        bool canUseUltra = aggression >= AggressionThresholdHeavy;
        bool useUltra    = canUseUltra && (Random.value < UltraChance(difficulty));

        List<RandomEventData> pool = (useUltra && ultraUnluckEvents.Count > 0)
            ? ultraUnluckEvents
            : (unluckEvents.Count > 0 ? unluckEvents : ultraUnluckEvents);

        List<RandomEventData> applicable = FilterApplicable(pool, player);
        if (applicable.Count == 0)
        {
            LogEvent($"[{competitorName}] Нет применимых событий для саботажа.");
            return;
        }

        RandomEventData ev = PickWeightedEvent(applicable);
        if (ev == null) return;

        ApplyEventToPlayer(ev, player, isSabotage: true);
        ShowNotification(ev.title, ev.description, true);
        LogSabotage(ev.title);
    }


    private void ApplyEventToPlayer(RandomEventData ev, PlayerStats player, bool isSabotage)
    {
        switch (ev.eventType)
        {
            case RandomEventType.AddBalance:
                int amount0 = isSabotage ? -Mathf.Abs(ev.balanceAmount) : ev.balanceAmount;
                player.Balance += amount0;
                player.CheckDebtState();
                player.UpdateUI();
                break;

            case RandomEventType.AddMood:
                int moodDelta = isSabotage ? -Mathf.Abs(ev.moodAmount) : ev.moodAmount;
                player.Mood = Mathf.Clamp(player.Mood + moodDelta, 0, 100);
                player.UpdateUI();
                break;

            case RandomEventType.ChangeJobIncomePercent:
                int salaryPct = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.jobIncomePercentBonus = Mathf.Clamp(
                    player.jobIncomePercentBonus + salaryPct, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.ChangeBusinessIncomePercent:
                int bizPct = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.businessIncomePercentBonus = Mathf.Clamp(
                    player.businessIncomePercentBonus + bizPct, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.ChangeRealtyIncomePercent:
                int realtyPct = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.realtyIncomePercentBonus = Mathf.Clamp(
                    player.realtyIncomePercentBonus + realtyPct, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.LoseBalancePercent:
                // В asset «потерял бумажник» percentAmount = 15 (процент потери)
                int loss5 = Mathf.RoundToInt(player.Balance * ev.percentAmount * 0.01f);
                player.Balance -= Mathf.Abs(loss5);
                player.CheckDebtState();
                player.UpdateUI();
                break;

            case RandomEventType.TemporaryMoneyAndTimePenalty:
                // В asset «серьёзная болезнь» balanceAmount = 200000 — берём Abs, т.к. это штраф
                player.Balance -= Mathf.Abs(ev.balanceAmount);
                player.CheckDebtState();
                if (ev.timePenalty > 0 && ev.durationPeriods > 0)
                    player.ApplyTimePenalty(ev.timePenalty, ev.durationPeriods);
                player.UpdateUI();
                break;

            case RandomEventType.AddRealty:
                if (!isSabotage && ev.realtyToAdd != null)
                    player.AddRealtyForFree(ev.realtyToAdd);
                break;

            case RandomEventType.ExtraPeriodIncome:
                // Банковский бонус/сбой: меняем все доходы на +/-10%
                int incomeBonus = isSabotage ? -10 : 10;
                player.jobIncomePercentBonus = Mathf.Clamp(
                    player.jobIncomePercentBonus + incomeBonus, BonusMin, BonusMax);
                player.businessIncomePercentBonus = Mathf.Clamp(
                    player.businessIncomePercentBonus + incomeBonus, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.BurnLargeDeposits:
                // Банкротство банка: вклады выше страхового порога сгорают
                if (isSabotage)
                    player.BurnLargeDeposits(1_400_000);
                break;

            case RandomEventType.BurnRandomBusinessOrRealty:
                // Пожар: уничтожает случайный актив
                if (isSabotage)
                    player.BurnRandomBusinessOrRealty();
                break;

            default:
                LogEvent($"[{competitorName}] Неизвестный eventType={ev.eventType}, " +
                         $"событие '{ev.title}' пропущено.");
                break;
        }
    }


    private bool IsEventApplicable(RandomEventData ev, PlayerStats player)
    {
        switch (ev.eventType)
        {
            case RandomEventType.ChangeBusinessIncomePercent:
                return HasBusiness(player);

            case RandomEventType.ChangeRealtyIncomePercent:
                return HasRealty(player);

            case RandomEventType.AddRealty:
                return ev.realtyToAdd != null;

            case RandomEventType.BurnRandomBusinessOrRealty:
                return HasBusiness(player) || HasRealty(player);

            default:
                return true;
        }
    }

    private List<RandomEventData> FilterApplicable(List<RandomEventData> pool, PlayerStats player)
    {
        var result = new List<RandomEventData>();
        foreach (var ev in pool)
            if (IsEventApplicable(ev, player))
                result.Add(ev);
        return result;
    }


    private RandomEventData PickEventFromPool(List<RandomEventData> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        return PickWeightedEvent(pool);
    }

    private RandomEventData PickWeightedEvent(List<RandomEventData> pool)
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

        int assetCount    = CountPlayerAssets(player);
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

    private void LogAggression(float aggression, int difficulty)
    {
#if UNITY_EDITOR
        string level = aggression < AggressionThresholdHelp  ? "НИЗКАЯ (помощь)"  :
                       aggression < AggressionThresholdLight ? "ЛЁГКАЯ"           :
                       aggression < AggressionThresholdHeavy ? "СРЕДНЯЯ"          : "ВЫСОКАЯ";
        string diff  = difficulty == 0 ? "Easy" : difficulty == 2 ? "Hard" : "Normal";
        Debug.Log($"[{competitorName}] Сложность: {diff} | Агрессия: {aggression:F2} → {level}");
#endif
    }
}
