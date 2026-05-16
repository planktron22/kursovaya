using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Competitor", menuName = "Kursovaya/Competitor AI")]
public class CompetitorAI : ScriptableObject
{
    public string competitorName = "Теневой Конкурент";
    public Sprite portrait;

    [Header("Статистика")]
    public int balance = 50000;
    public int passiveIncome = 800;

    // ─────────────────────────────────────────────
    // Константы агрессии
    // ─────────────────────────────────────────────

    // Пороги уровней агрессии (0.0 = ИИ сильнее, 1.0 = игрок доминирует)
    private const float AggressionThresholdHelp   = 0.25f; // ниже — ИИ помогает
    private const float AggressionThresholdLight  = 0.45f; // лёгкий саботаж
    private const float AggressionThresholdHeavy  = 0.70f; // тяжёлый саботаж

    // Веса факторов при расчёте агрессии
    private const float WeightBalance = 0.60f;
    private const float WeightAssets  = 0.40f;

    // Сколько активов считается "много" у игрока
    private const int AssetsConsideredMany = 4;

    // Рост ИИ
    private const int MaxPassiveIncomeGrowth = 400;
    private const int MaxPassiveIncome       = 5000;

    // ─────────────────────────────────────────────
    // Ограничители бонусов игрока
    // ─────────────────────────────────────────────

    private const int BonusMin = -20; // саботаж не опускает бонус ниже этого
    private const int BonusMax =  30; // помощь не поднимает бонус выше этого

    // ─────────────────────────────────────────────
    // Сила побочных эффектов — намеренно небольшая
    // ─────────────────────────────────────────────

    private const int   LightPenaltyMin          = 2;
    private const int   LightPenaltyMax          = 5;
    private const int   LightMoodPenaltyMin      = 3;
    private const int   LightMoodPenaltyMax      = 8;
    private const float MediumBalancePenaltyMin  = 0.01f; // 1% от баланса
    private const float MediumBalancePenaltyMax  = 0.03f; // 3% от баланса
    private const int   MediumPenaltyMin         = 3;
    private const int   MediumPenaltyMax         = 7;
    private const float HeavyBalancePenaltyMin   = 0.03f; // 3% от баланса
    private const float HeavyBalancePenaltyMax   = 0.07f; // 7% от баланса
    private const int   HelpBonusMin             = 3;
    private const int   HelpBonusMax             = 8;

    // ─────────────────────────────────────────────
    // Сообщения
    // ─────────────────────────────────────────────

    [SerializeField]
    private List<string> sabotageMessages = new List<string>
    {
        "Конкурент немного переманил ваших клиентов.",                  // 0 лёгкий
        "Небольшие слухи слегка подпортили настроение.",                // 1 лёгкий
        "Конкурент поднял цены — ваши расходы чуть выросли.",           // 2 средний
        "Один сотрудник ушёл к конкуренту — доход немного упал.",       // 3 средний
        "Конкурент занял контакт в нетворкинге — аренда чуть просела.", // 4 средний
        "Конкурент давит на рынок — баланс слегка просел.",             // 5 тяжёлый
        "Рыночное давление конкурента немного ударило по активам."      // 6 тяжёлый
    };

    [SerializeField]
    private List<string> helpMessages = new List<string>
    {
        "Волна интереса к рынку слегка подняла ваши доходы.",
        "Удачный период — настроение само чуть улучшилось.",
        "Рыночная конъюнктура сыграла немного в вашу пользу."
    };

    // ─────────────────────────────────────────────
    // Главный метод — вызывается из PlayerStats.ApplyPeriod()
    // ─────────────────────────────────────────────

    public void SimulateTurn(PlayerStats player)
    {
        // ИИ развивается каждый ход независимо от агрессии
        balance += passiveIncome;
        int growth = Random.Range(100, MaxPassiveIncomeGrowth);
        passiveIncome = Mathf.Min(passiveIncome + growth, MaxPassiveIncome);

        // Иногда ИИ сам получает негативное событие
        if (Random.value < 0.15f)
            passiveIncome = Mathf.Max(Mathf.RoundToInt(passiveIncome * 0.9f), 500);

        float aggression = CalculateAggression(player);
        LogAggression(aggression);

        if (aggression < AggressionThresholdHelp)
        {
            // Помощь тоже не гарантирована каждый ход — это побочный эффект
            if (Random.value < 0.35f)
                HelpPlayer(player);
        }
        else
        {
            // Шанс саботажа растёт с агрессией: 8% → 40%
            // Потолок низкий намеренно — эффекты фоновые, не основные
            float sabotageChance = Mathf.Lerp(0.08f, 0.40f,
                Mathf.InverseLerp(AggressionThresholdHelp, 1.0f, aggression));

            if (Random.value < sabotageChance)
                PerformSabotage(player, aggression);
        }
    }

    // ─────────────────────────────────────────────
    // Расчёт агрессии: 0.0 = ИИ впереди, 1.0 = игрок доминирует
    // ─────────────────────────────────────────────

    private float CalculateAggression(PlayerStats player)
    {
        float balanceFactor;
        if (balance <= 0)
        {
            // ИИ банкрот — игрок всегда "впереди"
            balanceFactor = 1.0f;
        }
        else if (player.Balance <= 0)
        {
            // Игрок в долгах — ИИ не помогает, но и не давит сильно
            // Возвращаем нейтральное значение между Help и Light порогами
            balanceFactor = 0.30f;
        }
        else
        {
            // Оба в плюсе — честное соотношение
            float total = (float)player.Balance + balance;
            balanceFactor = Mathf.Clamp01((float)player.Balance / total);
        }

        int assetCount = CountPlayerAssets(player);
        float assetFactor = Mathf.Clamp01((float)assetCount / AssetsConsideredMany);

        return Mathf.Clamp01(balanceFactor * WeightBalance + assetFactor * WeightAssets);
    }

    private int CountPlayerAssets(PlayerStats player)
    {
        int count = 0;
        foreach (var job in player.activeJobs)
            if (job.isBusiness || job.isRealty)
                count++;
        return count;
    }

    // ─────────────────────────────────────────────
    // Помощь игроку
    // ─────────────────────────────────────────────

    private void HelpPlayer(PlayerStats player)
    {
        int helpType = Random.Range(0, 3);
        switch (helpType)
        {
            case 0:
                player.jobIncomePercentBonus = Mathf.Min(BonusMax,
                    player.jobIncomePercentBonus + Random.Range(HelpBonusMin, HelpBonusMax));
                player.RecalculateIncomePublic();
                break;

            case 1:
                player.Mood = Mathf.Min(100,
                    player.Mood + Random.Range(HelpBonusMin, HelpBonusMax));
                player.UpdateUI();
                break;

            case 2:
                player.realtyIncomePercentBonus = Mathf.Min(BonusMax,
                    player.realtyIncomePercentBonus + Random.Range(HelpBonusMin, HelpBonusMax));
                player.RecalculateIncomePublic();
                break;
        }

        string msg = helpMessages[Random.Range(0, helpMessages.Count)];
        LogEvent($"<color=green>{competitorName} (помощь): {msg}</color>");
    }

    // ─────────────────────────────────────────────
    // Саботаж
    // ─────────────────────────────────────────────

    private void PerformSabotage(PlayerStats player, float aggression)
    {
        if (aggression < AggressionThresholdLight)
            PerformLightSabotage(player);
        else if (aggression < AggressionThresholdHeavy)
            PerformMediumSabotage(player);
        else
            PerformHeavySabotage(player);
    }

    private void PerformLightSabotage(PlayerStats player)
    {
        int type = Random.Range(0, 2);
        switch (type)
        {
            case 0:
                player.jobIncomePercentBonus = Mathf.Max(BonusMin,
                    player.jobIncomePercentBonus - Random.Range(LightPenaltyMin, LightPenaltyMax));
                player.RecalculateIncomePublic();
                LogSabotage(sabotageMessages[0]);
                break;

            case 1:
                player.Mood = Mathf.Max(0,
                    player.Mood - Random.Range(LightMoodPenaltyMin, LightMoodPenaltyMax));
                player.UpdateUI();
                LogSabotage(sabotageMessages[1]);
                break;
        }
    }

    private void PerformMediumSabotage(PlayerStats player)
    {
        int type = Random.Range(0, 3);
        switch (type)
        {
            case 0:
                int marketLoss = Mathf.RoundToInt(player.Balance *
                    Random.Range(MediumBalancePenaltyMin, MediumBalancePenaltyMax));
                player.Balance -= marketLoss; // не зажимаем в 0 — CheckDebtState сам разберётся
                player.CheckDebtState();
                player.UpdateUI();
                LogSabotage(sabotageMessages[2]);
                break;

            case 1:
                player.businessIncomePercentBonus = Mathf.Max(BonusMin,
                    player.businessIncomePercentBonus - Random.Range(MediumPenaltyMin, MediumPenaltyMax));
                player.RecalculateIncomePublic();
                LogSabotage(sabotageMessages[3]);
                break;

            case 2:
                player.realtyIncomePercentBonus = Mathf.Max(BonusMin,
                    player.realtyIncomePercentBonus - Random.Range(MediumPenaltyMin, MediumPenaltyMax));
                player.RecalculateIncomePublic();
                LogSabotage(sabotageMessages[4]);
                break;
        }
    }

    // Тяжёлый: 3–7% от баланса + небольшой штраф к бизнес-бонусу
    // BurnRandomBusinessOrRealty убран намеренно — слишком разрушительно
    private void PerformHeavySabotage(PlayerStats player)
    {
        int heavyLoss = Mathf.RoundToInt(player.Balance *
            Random.Range(HeavyBalancePenaltyMin, HeavyBalancePenaltyMax));
        player.Balance -= heavyLoss; // не зажимаем в 0 — CheckDebtState сам разберётся
        player.CheckDebtState();
        player.UpdateUI();

        player.businessIncomePercentBonus = Mathf.Max(BonusMin,
            player.businessIncomePercentBonus - Random.Range(LightPenaltyMin, LightPenaltyMax));
        player.RecalculateIncomePublic();

        // Индексы 5 и 6 — тяжёлые сообщения. Если добавляешь новые — обновляй этот Range
        int msgIndex = Random.Range(5, sabotageMessages.Count);
        LogSabotage(sabotageMessages[msgIndex]);
    }

    // ─────────────────────────────────────────────
    // UI
    // ─────────────────────────────────────────────

    public string GetLeaderText(PlayerStats player)
    {
        if (balance > player.Balance * 1.3f)
            return $"<color=red>{competitorName} лидирует!</color>";
        else if (balance > player.Balance)
            return $"{competitorName} немного впереди";
        else
            return $"Вы опережаете {competitorName}";
    }

    // ─────────────────────────────────────────────
    // Логирование — только в Editor
    // ─────────────────────────────────────────────

    private void LogSabotage(string message)
    {
#if UNITY_EDITOR
        Debug.Log($"<color=red>{competitorName}: {message}</color>");
#endif
    }

    private void LogEvent(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#endif
    }

    private void LogAggression(float aggression)
    {
#if UNITY_EDITOR
        string level = aggression < AggressionThresholdHelp  ? "НИЗКАЯ (помощь)"  :
                       aggression < AggressionThresholdLight ? "ЛЁГКАЯ"           :
                       aggression < AggressionThresholdHeavy ? "СРЕДНЯЯ"          : "ВЫСОКАЯ";
        Debug.Log($"[{competitorName}] Агрессия: {aggression:F2} → {level}");
#endif
    }
}
