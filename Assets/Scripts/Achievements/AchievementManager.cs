using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Отдельная система достижений. Ничего не меняет в чужих скриптах:
/// только читает PlayerStats и PlayerMovement и сохраняет открытые достижения в PlayerPrefs.
/// </summary>
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private static readonly string[] AllAchievementIds =
    {
        "first_move",
        "first_period",
        "first_year",
        "positive_income",
        "safety_cushion",
        "debt",
        "low_mood",
        "no_time",
        "depression",
        "first_job",
        "first_business",
        "first_realty",
        "first_invest",
        "first_skill",
        "first_person",
        "first_deposit",
        "first_credit"
    };

    [Header("Links")]
    public PlayerStats player;
    public PlayerMovement movement;
    public AchievementToastUI toastUI;
    public AchievementPanelUI panelUI;

    [Header("Settings")]
    public bool autoFindLinks = true;
    public float checkInterval = 0.25f;

    private readonly List<AchievementInfo> achievements = new List<AchievementInfo>();
    private float checkTimer;

    private int startTile;
    private bool wasMoving;
    private int movesStarted;
    private int periodsPassed;
    private int previousTile;
    private int previousAge;
    private int previousBalance;
    private int previousMood;

    public IReadOnlyList<AchievementInfo> Achievements => achievements;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CreateAchievementList();
        LoadUnlockedAchievements();
    }

    void Start()
    {
        FindLinksIfNeeded();

        if (movement != null)
        {
            startTile = movement.currentTile;
            previousTile = movement.currentTile;
        }

        if (player != null)
        {
            previousAge = player.Age;
            previousBalance = player.Balance;
            previousMood = player.Mood;
        }

        RefreshPanel();
    }

    void Update()
    {
        FindLinksIfNeeded();
        TrackMovementAndPeriods();

        checkTimer -= Time.deltaTime;
        if (checkTimer <= 0f)
        {
            checkTimer = checkInterval;
            CheckAchievements();
        }
    }

    void FindLinksIfNeeded()
    {
        if (!autoFindLinks)
            return;

        if (player == null)
            player = FindObjectOfType<PlayerStats>();

        if (movement == null)
            movement = FindObjectOfType<PlayerMovement>();

        if (toastUI == null)
            toastUI = FindObjectOfType<AchievementToastUI>();

        if (panelUI == null)
            panelUI = FindObjectOfType<AchievementPanelUI>();
    }

    void CreateAchievementList()
    {
        achievements.Clear();

        Add("first_move", "Первый шаг", "Сделать первый ход по игровому полю.");
        Add("first_period", "Новый период", "Пройти клетку периода и получить перерасчёт финансов.");
        Add("first_year", "Круг жизни", "Пройти полный круг по игровому полю.");

        Add("positive_income", "В плюс", "Получить положительную чистую прибыль.");
        Add("safety_cushion", "Финансовая подушка", "Накопить 1 000 000 рублей на балансе.");
        Add("debt", "Жизнь в долг", "Уйти в отрицательный баланс.");

        Add("low_mood", "На нервах", "Опустить настроение до 20 или ниже.");
        Add("no_time", "Нет времени", "Оставить 10 или меньше часов свободного времени.");
        Add("depression", "Тяжёлый период", "Довести персонажа до депрессии.");

        Add("first_job", "Первая работа", "Получить первую работу.");
        Add("first_business", "Бизнесмен", "Открыть первый бизнес.");
        Add("first_realty", "Рантье", "Получить или купить недвижимость.");
        Add("first_invest", "Инвестор", "Купить первые акции.");

        Add("first_skill", "Саморазвитие", "Изучить первый навык.");
        Add("first_person", "Полезное знакомство", "Успешно познакомиться с первым человеком.");

        Add("first_deposit", "Вкладчик", "Открыть первый банковский вклад.");
        Add("first_credit", "Кредит доверия", "Получить первый кредит в банке.");
    }

    void Add(string id, string title, string description)
    {
        achievements.Add(new AchievementInfo(id, title, description));
    }

    void TrackMovementAndPeriods()
    {
        if (movement == null)
            return;

        if (!wasMoving && movement.isMoving)
        {
            movesStarted++;
            Unlock("first_move");
        }

        wasMoving = movement.isMoving;

        if (movement.currentTile != previousTile)
        {
            Tile tile = null;

            if (movement.tiles != null && movement.currentTile >= 0 && movement.currentTile < movement.tiles.Length)
            {
                Transform tileTransform = movement.tiles[movement.currentTile];
                if (tileTransform != null)
                    tile = tileTransform.GetComponent<Tile>();
            }

            if (tile != null && tile.tileType == TileType.Period)
            {
                periodsPassed++;
                Unlock("first_period");
            }

            previousTile = movement.currentTile;
        }
    }

    void CheckAchievements()
    {
        if (player == null)
            return;

        if (movement != null && movement.currentTile != startTile)
            Unlock("first_move");

        if (player.Age < previousAge)
            Unlock("first_year");

        if (player.NetIncome > 0)
            Unlock("positive_income");

        if (player.Balance >= 1000000)
            Unlock("safety_cushion");

        if (player.Balance < 0 || player.debtMode)
            Unlock("debt");

        if (player.Mood <= 20)
            Unlock("low_mood");

        if (player.FreeTime <= 10)
            Unlock("no_time");

        if (player.isDepressed)
            Unlock("depression");

        if (HasJob())
            Unlock("first_job");

        if (HasBusiness())
            Unlock("first_business");

        if (HasRealty())
            Unlock("first_realty");

        if (HasInvest())
            Unlock("first_invest");

        if (player.learnedSkills != null && player.learnedSkills.Count > 0)
            Unlock("first_skill");

        if (player.knownPeople != null && player.knownPeople.Count > 0)
            Unlock("first_person");

        if (HasDeposit())
            Unlock("first_deposit");

        if (HasCredit())
            Unlock("first_credit");

        previousAge = player.Age;
        previousBalance = player.Balance;
        previousMood = player.Mood;
    }

    bool HasJob()
    {
        if (player.activeJobs == null)
            return false;

        foreach (var item in player.activeJobs)
        {
            if (item != null && !item.isBusiness && !item.isRealty)
                return true;
        }

        return false;
    }

    bool HasBusiness()
    {
        if (player.activeJobs == null)
            return false;

        foreach (var item in player.activeJobs)
        {
            if (item != null && item.isBusiness)
                return true;
        }

        return false;
    }

    bool HasRealty()
    {
        if (player.activeJobs == null)
            return false;

        foreach (var item in player.activeJobs)
        {
            if (item != null && item.isRealty)
                return true;
        }

        return false;
    }

    bool HasInvest()
    {
        if (player.activeInvests == null)
            return false;

        foreach (var invest in player.activeInvests)
        {
            if (invest != null && invest.amount > 0)
                return true;
        }

        return false;
    }

    bool HasDeposit()
    {
        if (player.activeBankDeals == null)
            return false;

        foreach (var deal in player.activeBankDeals)
        {
            if (deal != null && deal.isDeposit)
                return true;
        }

        return false;
    }

    bool HasCredit()
    {
        if (player.activeBankDeals == null)
            return false;

        foreach (var deal in player.activeBankDeals)
        {
            if (deal != null && !deal.isDeposit)
                return true;
        }

        return false;
    }

    public void Unlock(string id)
    {
        AchievementInfo achievement = GetAchievement(id);

        if (achievement == null || achievement.unlocked)
            return;

        achievement.unlocked = true;
        PlayerPrefs.SetInt(GetPrefsKey(id), 1);
        PlayerPrefs.Save();

        Debug.Log("Достижение открыто: " + achievement.title);

        if (toastUI != null)
            toastUI.Show(achievement);

        RefreshPanel();
    }

    public AchievementInfo GetAchievement(string id)
    {
        foreach (AchievementInfo achievement in achievements)
        {
            if (achievement.id == id)
                return achievement;
        }

        return null;
    }

    public int GetUnlockedCount()
    {
        int count = 0;

        foreach (AchievementInfo achievement in achievements)
        {
            if (achievement.unlocked)
                count++;
        }

        return count;
    }

    public int GetTotalCount()
    {
        return achievements.Count;
    }

    public void ResetAchievements()
    {
        ResetSavedAchievements();

        foreach (AchievementInfo achievement in achievements)
            achievement.unlocked = false;

        RefreshPanel();
    }

    public static void ResetSavedAchievements()
    {
        foreach (string id in AllAchievementIds)
            PlayerPrefs.DeleteKey(GetPrefsKeyStatic(id));

        PlayerPrefs.Save();
        Debug.Log("Достижения сброшены для новой игры");
    }

    void LoadUnlockedAchievements()
    {
        foreach (AchievementInfo achievement in achievements)
        {
            achievement.unlocked = PlayerPrefs.GetInt(GetPrefsKey(achievement.id), 0) == 1;
        }
    }

    string GetPrefsKey(string id)
    {
        return GetPrefsKeyStatic(id);
    }

    static string GetPrefsKeyStatic(string id)
    {
        return "CourseworkAchievement_" + id;
    }

    void RefreshPanel()
    {
        if (panelUI != null)
            panelUI.Refresh();
    }
}
