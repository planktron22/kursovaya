#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Создает готовую панель достижений прямо в открытой сцене.
/// Используется только в редакторе Unity, в билд игры не попадает.
/// </summary>
public static class AchievementPanelSceneBuilder
{
    private struct PreviewAchievement
    {
        public string id;
        public string title;
        public string description;
        public bool opened;

        public PreviewAchievement(string id, string title, string description, bool opened)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.opened = opened;
        }
    }

    private static readonly PreviewAchievement[] PreviewAchievements =
    {
        new PreviewAchievement("first_move", "Первый шаг", "Сделать первый ход по игровому полю.", true),
        new PreviewAchievement("first_period", "Новый период", "Пройти клетку периода и получить перерасчёт финансов.", false),
        new PreviewAchievement("first_year", "Круг жизни", "Пройти полный круг по игровому полю.", false),
        new PreviewAchievement("positive_income", "В плюс", "Получить положительную чистую прибыль.", true),
        new PreviewAchievement("safety_cushion", "Финансовая подушка", "Накопить 1 000 000 рублей на балансе.", true),
        new PreviewAchievement("debt", "Жизнь в долг", "Уйти в отрицательный баланс.", false),
        new PreviewAchievement("low_mood", "На нервах", "Опустить настроение до 20 или ниже.", false),
        new PreviewAchievement("no_time", "Нет времени", "Оставить 10 или меньше часов свободного времени.", false),
        new PreviewAchievement("depression", "Тяжёлый период", "Довести персонажа до депрессии.", false),
        new PreviewAchievement("first_job", "Первая работа", "Получить первую работу.", false),
        new PreviewAchievement("first_business", "Бизнесмен", "Открыть первый бизнес.", false),
        new PreviewAchievement("first_realty", "Рантье", "Получить или купить недвижимость.", false),
        new PreviewAchievement("first_invest", "Инвестор", "Купить первые акции.", false),
        new PreviewAchievement("first_skill", "Саморазвитие", "Изучить первый навык.", false),
        new PreviewAchievement("first_person", "Полезное знакомство", "Успешно познакомиться с первым человеком.", true),
        new PreviewAchievement("first_deposit", "Вкладчик", "Открыть первый банковский вклад.", false),
        new PreviewAchievement("first_credit", "Кредит доверия", "Получить первый кредит в банке.", false),
        new PreviewAchievement("ai_first_event", "ИИ вмешался", "Получить первое событие от ИИ-бота соперника.", false),
        new PreviewAchievement("ai_first_help", "Неожиданная помощь", "Получить полезное событие от ИИ-бота.", false),
        new PreviewAchievement("ai_first_sabotage", "Саботаж", "Получить вредное событие от ИИ-бота.", false)
    };

    private static readonly Color PanelColor = new Color(0.67f, 0.64f, 0.64f, 0.55f);
    private static readonly Color ScrollColor = new Color(1f, 1f, 1f, 0.18f);
    private static readonly Color TextColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    private static readonly Color OpenedColor = new Color(0.50f, 0.82f, 0.50f, 0.78f);
    private static readonly Color ClosedColor = new Color(0.75f, 0.75f, 0.75f, 0.62f);
    private static readonly Color ButtonColor = new Color(0.121f, 0.542f, 0.736f, 1f);

    [MenuItem("Tools/Achievements/Recreate Achievement Panel In Scene")]
    public static void RecreateAchievementPanelInScene()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        DeleteSceneObject("AchievementsPanel");
        DeleteSceneObject("AchievementsButton");
        DeleteSceneObject("AchievementButton");

        Button toggleButton = CreateButton(canvas.transform, "AchievementsButton", "Достижения", new Vector2(-25f, 28f), new Vector2(165f, 46f), false);

        GameObject panel = CreateUIObject("AchievementsPanel", canvas.transform, typeof(Image));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(920f, 610f);
        panel.GetComponent<Image>().color = PanelColor;

        Text title = CreateText(panel.transform, "Title", "Достижения 3/20", new Vector2(35f, -24f), new Vector2(650f, 46f), 30, FontStyle.Normal);
        title.alignment = TextAnchor.MiddleLeft;

        Button closeButton = CreateButton(panel.transform, "CloseButton", "X", new Vector2(-24f, -20f), new Vector2(48f, 44f), true);

        GameObject scrollObject = CreateUIObject("AchievementsScrollView", panel.transform, typeof(Image), typeof(ScrollRect));
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(35f, 35f);
        scrollRectTransform.offsetMax = new Vector2(-35f, -95f);
        scrollObject.GetComponent<Image>().color = ScrollColor;

        GameObject viewport = CreateUIObject("Viewport", scrollObject.transform, typeof(Image), typeof(Mask));
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = new Vector2(-24f, 0f);
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.03f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = CreateUIObject("Content", viewport.transform, typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject template = CreateAchievementRow(content.transform, "AchievementItemTemplate", "Название достижения", "Описание достижения будет находиться здесь.", "Закрыто", false);
        template.SetActive(false);

        for (int i = 0; i < PreviewAchievements.Length; i++)
        {
            PreviewAchievement achievement = PreviewAchievements[i];
            string rowName = "AchievementPreview_" + (i + 1).ToString("00") + "_" + achievement.id;
            CreateAchievementRow(content.transform, rowName, achievement.title, achievement.description, achievement.opened ? "Открыто" : "Закрыто", achievement.opened);
        }

        Scrollbar verticalScrollbar = CreateVerticalScrollbar(scrollObject.transform);

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.verticalScrollbar = verticalScrollbar;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        scrollRect.verticalScrollbarSpacing = 4f;
        scrollRect.scrollSensitivity = 25f;

        AchievementPanelUI panelUI = Object.FindObjectOfType<AchievementPanelUI>();
        if (panelUI == null)
        {
            GameObject systemObject = GameObject.Find("AchievementSystem");
            if (systemObject == null)
                systemObject = new GameObject("AchievementSystem");

            panelUI = systemObject.AddComponent<AchievementPanelUI>();
        }

        panelUI.panelRoot = panel;
        panelUI.contentParent = content.transform;
        panelUI.headerText = title;
        panelUI.toggleButton = toggleButton;
        panelUI.closeButton = closeButton;
        panelUI.itemTemplate = template;
        panelUI.createUIOnStart = false;
        panelUI.autoBindSceneUI = true;
        panelUI.hidePanelOnStart = true;

        AchievementManager manager = Object.FindObjectOfType<AchievementManager>();
        if (manager != null)
            manager.panelUI = panelUI;

        Selection.activeGameObject = panel;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Achievement panel recreated in the current scene. Preview rows are visible in Edit Mode; real rows are generated when the game starts.");
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObject, "Create Canvas");

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

        return canvas;
    }

    private static void DeleteSceneObject(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
            Undo.DestroyObjectImmediate(obj);
    }

    private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        foreach (System.Type component in components)
            obj.AddComponent(component);

        obj.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
        return obj;
    }

    private static GameObject CreateAchievementRow(Transform parent, string rowName, string title, string description, string status, bool opened)
    {
        GameObject row = CreateUIObject(rowName, parent, typeof(Image), typeof(LayoutElement));
        row.GetComponent<Image>().color = opened ? OpenedColor : ClosedColor;

        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0f, 82f);

        LayoutElement layoutElement = row.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 82f;
        layoutElement.minHeight = 82f;

        Text titleText = CreateText(row.transform, "TitleText", title, new Vector2(18f, -9f), new Vector2(560f, 26f), 17, FontStyle.Bold);
        titleText.color = opened ? new Color(0.05f, 0.30f, 0.05f, 1f) : TextColor;

        CreateText(row.transform, "DescriptionText", description, new Vector2(18f, -38f), new Vector2(680f, 34f), 13, FontStyle.Normal);

        Text statusText = CreateText(row.transform, "StatusText", status, new Vector2(-150f, -24f), new Vector2(132f, 28f), 15, FontStyle.Bold);
        RectTransform statusRect = statusText.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(1f, 1f);
        statusRect.anchorMax = new Vector2(1f, 1f);
        statusRect.pivot = new Vector2(1f, 1f);
        statusText.alignment = TextAnchor.MiddleRight;
        statusText.color = opened ? new Color(0.05f, 0.30f, 0.05f, 1f) : TextColor;

        return row;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, bool topRight)
    {
        GameObject obj = CreateUIObject(name, parent, typeof(Image), typeof(Button));
        RectTransform rect = obj.GetComponent<RectTransform>();

        if (topRight)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
        }
        else
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
        }

        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = obj.GetComponent<Image>();
        image.color = topRight ? Color.white : ButtonColor;

        Text text = CreateText(obj.transform, "Text", label, Vector2.zero, size, 16, FontStyle.Normal);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = topRight ? TextColor : Color.white;

        return obj.GetComponent<Button>();
    }

    private static Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, int fontSize, FontStyle style)
    {
        GameObject obj = CreateUIObject(name, parent, typeof(Text));
        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.text = value;
        text.color = TextColor;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        return text;
    }

    private static Scrollbar CreateVerticalScrollbar(Transform parent)
    {
        GameObject scrollbarObject = CreateUIObject("Scrollbar Vertical", parent, typeof(Image), typeof(Scrollbar));
        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.pivot = new Vector2(1f, 1f);
        scrollbarRect.offsetMin = new Vector2(-20f, 0f);
        scrollbarRect.offsetMax = Vector2.zero;
        scrollbarObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.22f);

        GameObject slidingArea = CreateUIObject("Sliding Area", scrollbarObject.transform);
        RectTransform slidingRect = slidingArea.GetComponent<RectTransform>();
        slidingRect.anchorMin = Vector2.zero;
        slidingRect.anchorMax = Vector2.one;
        slidingRect.offsetMin = new Vector2(2f, 2f);
        slidingRect.offsetMax = new Vector2(-2f, -2f);

        GameObject handle = CreateUIObject("Handle", slidingArea.transform, typeof(Image));
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = Vector2.zero;
        handleRect.offsetMax = Vector2.zero;
        Image handleImage = handle.GetComponent<Image>();
        handleImage.color = new Color(0.50f, 0.50f, 0.50f, 0.65f);

        Scrollbar scrollbar = scrollbarObject.GetComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.targetGraphic = handleImage;
        scrollbar.handleRect = handleRect;
        scrollbar.size = 0.35f;

        return scrollbar;
    }
}
#endif
