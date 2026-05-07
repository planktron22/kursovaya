using UnityEngine;

public class RandomEventManager : MonoBehaviour
{
    public OpportunityDatabase database;
    public PlayerStats player;

    public void TriggerEvent(TileType tileType)
    {
        RandomEventData[] events = GetEventsByTile(tileType);

        if (events == null || events.Length == 0)
        {
            Debug.Log("Нет событий для типа: " + tileType);
            return;
        }

        RandomEventData selectedEvent = GetWeightedRandomEvent(events);

        if (selectedEvent == null)
        {
            Debug.LogError("Событие не выбрано");
            return;
        }

        ApplyEvent(selectedEvent);
    }

    RandomEventData[] GetEventsByTile(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.Luck:
                return database.luckEvents;

            case TileType.Unluck:
                return database.unluckEvents;

            case TileType.UltraLuck:
                return database.ultraLuckEvents;

            case TileType.UltraUnluck:
                return database.ultraUnluckEvents;

            default:
                return null;
        }
    }

    RandomEventData GetWeightedRandomEvent(RandomEventData[] events)
    {
        int totalProbability = 0;

        foreach (var e in events)
            totalProbability += e.probability;

        int roll = Random.Range(1, totalProbability + 1);
        int current = 0;

        foreach (var e in events)
        {
            current += e.probability;

            if (roll <= current)
                return e;
        }

        return events[events.Length - 1];
    }

    void ApplyEvent(RandomEventData e)
    {
        Debug.Log("Событие: " + e.title);

        switch (e.eventType)
        {
            case RandomEventType.AddBalance:
                player.Balance += e.balanceAmount;
                player.CheckDebtState();
                player.UpdateUI();
                break;

            case RandomEventType.AddMood:
                player.Mood += e.moodAmount;
                player.Mood = Mathf.Clamp(player.Mood, 0, 100);
                player.UpdateUI();
                break;

            case RandomEventType.ChangeJobIncomePercent:
                player.jobIncomePercentBonus += e.percentAmount;
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.ChangeBusinessIncomePercent:
                player.businessIncomePercentBonus += e.percentAmount;
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.ChangeRealtyIncomePercent:
                player.realtyIncomePercentBonus += e.percentAmount;
                player.RecalculateIncomePublic();
                break;

            case RandomEventType.LoseBalancePercent:
                int loss = Mathf.RoundToInt(player.Balance * (e.percentAmount / 100f));
                player.Balance -= loss;
                player.CheckDebtState();
                player.UpdateUI();
                break;

            case RandomEventType.TemporaryMoneyAndTimePenalty:
                player.Balance -= e.balanceAmount;
                player.FreeTime -= e.timePenalty;

                if (player.FreeTime < 0)
                    player.FreeTime = 0;

                player.temporaryEffects.Add(new TemporaryEffect
                {
                    title = e.title,
                    remainingPeriods = e.durationPeriods,
                    timePenalty = e.timePenalty
                });

                player.CheckDebtState();
                player.UpdateUI();
                break;

            case RandomEventType.AddRealty:
                player.AddRealtyForFree(e.realtyToAdd);
                break;

            case RandomEventType.ExtraPeriodIncome:
                player.Balance += player.NetIncome;
                player.CheckDebtState();
                player.UpdateUI();
                break;

            case RandomEventType.BurnLargeDeposits:
                player.BurnLargeDeposits(1400000);
                break;

            case RandomEventType.BurnRandomBusinessOrRealty:
                player.BurnRandomBusinessOrRealty();
                break;
        }
    }
}