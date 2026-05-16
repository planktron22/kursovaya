using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Competitor", menuName = "Kursovaya/Competitor AI")]
public class CompetitorAI : ScriptableObject
{
    public string competitorName = "Теневой Конкурент";
    public Sprite portrait;

    [Header("Статистика")]
    public float money = 50000f;
    public float passiveIncome = 800f;
    public int reputation = 50;

    [Header("Саботаж")]
    [Range(0f, 1f)] public float sabotageChance = 0.35f; // 35% шанс на саботаж каждый ход

    private List<string> sabotageMessages = new List<string>
    {
        "Конкурент переманил нескольких ваших клиентов.",
        "Распространились слухи о вашей компании — настроение упало.",
        "Конкурент купил объект раньше вас и поднял цену на рынке.",
        "Ваш сотрудник ушёл к конкуренту.",
        "Конкурент занял выгодный контакт в нетворкинге."
    };

    public void SimulateTurn(PlayerStats player)
    {
        // ИИ развивается
        money += passiveIncome;
        passiveIncome += Random.Range(100f, 400f); // постепенно растёт

        // === САБОТАЖ ===
        if (Random.value < sabotageChance)
        {
            PerformSabotage(player);
        }

        // Иногда ИИ тоже получает негативные события
        if (Random.value < 0.15f)
        {
            passiveIncome = Mathf.Max(passiveIncome * 0.9f, 500f);
        }
    }

    private void PerformSabotage(PlayerStats player)
    {
        int sabotageType = Random.Range(0, 5);

        switch (sabotageType)
        {
            case 0: // Переманивание клиентов
                if (player.businessIncome > 0)
                {
                    float loss = player.businessIncome * Random.Range(0.15f, 0.3f);
                    player.businessIncome -= loss;
                    Debug.Log($"<color=red>{competitorName}: {sabotageMessages[0]}</color>");
                }
                break;

            case 1: // Слухи (настроение)
                player.mood = Mathf.Max(0, player.mood - Random.Range(8, 18));
                Debug.Log($"<color=red>{competitorName}: {sabotageMessages[1]}</color>");
                break;

            case 2: // Поднятие цены на рынке
                Debug.Log($"<color=red>{competitorName}: {sabotageMessages[2]}</color>");
                // Можно добавить глобальный модификатор цен
                break;

            case 3: // Увод сотрудника
                player.AddEmployeeLoss();
                Debug.Log($"<color=red>{competitorName}: {sabotageMessages[3]}</color>");
                break;

            case 4: // Захват контакта
                Debug.Log($"<color=red>{competitorName}: {sabotageMessages[4]}</color>");
                break;
        }
    }

    public string GetLeaderText(PlayerStats player)
    {
        if (money > player.money * 1.3f)
            return $"<color=red>{competitorName} лидирует!</color>";
        else if (money > player.money)
            return $"{competitorName} немного впереди";
        else
            return $"Вы опережаете {competitorName}";
    }
}