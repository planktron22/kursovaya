using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Competitor", menuName = "Kursovaya/Competitor AI")]
public class CompetitorAI : ScriptableObject
{
    public string competitorName = "Судьба";

    [Header("Пулы событий")]
    public List<RandomEventData> luckEvents      = new List<RandomEventData>();
    public List<RandomEventData> ultraLuckEvents  = new List<RandomEventData>();
    public List<RandomEventData> unluckEvents     = new List<RandomEventData>();
    public List<RandomEventData> ultraUnluckEvents = new List<RandomEventData>();

    // ─── Константы ───────────────────────────────────────────────────────────
    private const int   TilesPerLap        = 48;   // клеток на поле
    private const int   MaxEventsPerLap    = 2;    // максимум событий за круг
    private const int   BonusMin           = -20;
    private const int   BonusMax           =  30;

    // Пороги капитала
    private const float ThresholdHelp      = 0.30f;  // < 30%  → только помощь
    private const float ThresholdNeutral   = 0.70f;  // 30–70% → ничего
    private const float ThresholdEven      = 1.50f;  // 70–150% → 50/50
    private const float ThresholdMedium    = 2.00f;  // 150–200% → 60% плохих / 40% хороших
                                                     // > 200%   → 70% плохих / 30% хороших

    // ─── Состояние (сбрасывается каждый круг) ────────────────────────────────
    private int  eventsThisLap   = 0;
    private int  stepsSinceReset = 0;
    private int  startBalance    = -1;   // запоминаем при первом вызове

    private bool hadEventThisLap = false;


    // ─────────────────────────────────────────────────────────────────────────
    // Главный метод — вызывается из PlayerMovement когда игрок встал на Empty
    // ─────────────────────────────────────────────────────────────────────────
    public void SimulateTurn(PlayerStats player)
    {
        // Запоминаем стартовый баланс один раз
        if (startBalance < 0)
            startBalance = player.Balance;

        // Считаем шаги и сбрасываем счётчик круга
        stepsSinceReset++;
        if (stepsSinceReset >= TilesPerLap)
        {
            stepsSinceReset = 0;
            eventsThisLap   = 0;
        }

        // Не больше 2 событий за круг
        if (eventsThisLap >= MaxEventsPerLap)
            return;

        float ratio = startBalance > 0
            ? (float)player.Balance / startBalance
            : 1f;

        // < 30% стартового → только помощь
        if (ratio < ThresholdHelp)
        {
            TriggerHelp(player);
            return;
        }

        // 30–70%  → 30% плохих / 70% хороших
        // 70–150%  → 50% плохих / 50% хороших
        // 150–200% → 60% плохих / 40% хороших
        // > 200%   → 70% плохих / 30% хороших
        float badChance = ratio < ThresholdNeutral ? 0.30f :
                          ratio < ThresholdEven    ? 0.50f :
                          ratio < ThresholdMedium  ? 0.60f : 0.70f;

        if (Random.value < badChance)
            TriggerSabotage(player, ratio);
        else
            TriggerHelp(player);
    }

    // ─── Помощь ──────────────────────────────────────────────────────────────
    private void TriggerHelp(PlayerStats player)
    {
        // 1 к 5 шанс на ultraLuck
        bool useUltra = Random.value < 0.20f && ultraLuckEvents.Count > 0;
        List<RandomEventData> pool = useUltra ? ultraLuckEvents
            : (luckEvents.Count > 0 ? luckEvents : ultraLuckEvents);

        RandomEventData ev = PickWeighted(pool);
        if (ev == null) return;

        ApplyEvent(ev, player, false);
        ShowNotification(ev, false);
        eventsThisLap++;
        hadEventThisLap = true;
        Log($"<color=green>Помощь: {ev.title}</color>");
    }

    // ─── Саботаж ─────────────────────────────────────────────────────────────
    private void TriggerSabotage(PlayerStats player, float ratio)
    {
        // 1 к 5 шанс на ultraUnluck
        bool useUltra = Random.value < 0.20f && ultraUnluckEvents.Count > 0;

        List<RandomEventData> pool = useUltra ? ultraUnluckEvents
            : (unluckEvents.Count > 0 ? unluckEvents : ultraUnluckEvents);

        List<RandomEventData> applicable = FilterApplicable(pool, player);
        RandomEventData ev = PickWeighted(applicable);
        if (ev == null) return;

        ApplyEvent(ev, player, true);
        ShowNotification(ev, true);
        eventsThisLap++;
        hadEventThisLap = true;
        Log($"<color=red>Саботаж: {ev.title}</color>");
    }

    // ─── Применение события ──────────────────────────────────────────────────
    private void ApplyEvent(RandomEventData ev, PlayerStats player, bool isSabotage)
    {
        switch (ev.eventType)
        {
            case RandomEventType.AddBalance:
                int amount = isSabotage ? -Mathf.Abs(ev.balanceAmount) : ev.balanceAmount;
                player.Balance += amount;
                player.CheckDebtState();
                player.UpdateUI();
                break;

            case RandomEventType.AddMood:
                int mood = isSabotage ? -Mathf.Abs(ev.moodAmount) : ev.moodAmount;
                player.Mood = Mathf.Clamp(player.Mood + mood, 0, 100);
                player.UpdateUI();
                break;

            case RandomEventType.ChangeJobIncomePercent:
                int jp = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.jobIncomePercentBonus = Mathf.Clamp(player.jobIncomePercentBonus + jp, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.ChangeBusinessIncomePercent:
                int bp = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.businessIncomePercentBonus = Mathf.Clamp(player.businessIncomePercentBonus + bp, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.ChangeRealtyIncomePercent:
                int rp = isSabotage ? -Mathf.Abs(ev.percentAmount) : ev.percentAmount;
                player.realtyIncomePercentBonus = Mathf.Clamp(player.realtyIncomePercentBonus + rp, BonusMin, BonusMax);
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.LoseBalancePercent:
                int loss = Mathf.RoundToInt(player.Balance * ev.percentAmount * 0.01f);
                player.Balance -= Mathf.Abs(loss);
                player.CheckDebtState();
                player.UpdateUI();
                break;

            case RandomEventType.TemporaryMoneyAndTimePenalty:
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

            case RandomEventType.BurnLargeDeposits:
                if (isSabotage) player.BurnLargeDeposits(1_400_000);
                break;

            case RandomEventType.BurnRandomBusinessOrRealty:
                if (isSabotage) player.BurnRandomBusinessOrRealty();
                break;
        }
    }

    // ─── Фильтр применимых событий ───────────────────────────────────────────
    private bool IsApplicable(RandomEventData ev, PlayerStats player)
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
            if (IsApplicable(ev, player)) result.Add(ev);
        return result;
    }

    // ─── Взвешенный выбор события ────────────────────────────────────────────
    private RandomEventData PickWeighted(List<RandomEventData> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        int total = 0;
        foreach (var ev in pool) total += Mathf.Max(1, ev.probability);

        int roll = Random.Range(0, total);
        int cum  = 0;
        foreach (var ev in pool)
        {
            cum += Mathf.Max(1, ev.probability);
            if (roll < cum) return ev;
        }
        return pool[pool.Count - 1];
    }

    // ─── Уведомление ─────────────────────────────────────────────────────────
    private void ShowNotification(RandomEventData ev, bool isSabotage)
    {
        UIManager ui = Object.FindObjectOfType<UIManager>();
        if (ui != null)
            ui.ShowCompetitorEvent(competitorName, ev.title, ev.description, isSabotage);
    }

    // ─── Вспомогательные ─────────────────────────────────────────────────────
    private bool HasBusiness(PlayerStats p)
    {
        foreach (var j in p.activeJobs) if (j.isBusiness) return true;
        return false;
    }

    private bool HasRealty(PlayerStats p)
    {
        foreach (var j in p.activeJobs) if (j.isRealty) return true;
        return false;
    }

    private void Log(string msg)
    {
#if UNITY_EDITOR
        Debug.Log($"[{competitorName}] {msg}");
#endif
    }
}

