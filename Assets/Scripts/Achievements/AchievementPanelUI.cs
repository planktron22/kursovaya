using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Панель достижений. Работает с UI, который уже лежит в сцене.
/// Ничего не меняет в чужих системах: только отображает данные AchievementManager.
/// </summary>
public class AchievementPanelUI : MonoBehaviour
{
    [Header("Ready UI in scene")]
    public GameObject panelRoot;
    public Transform contentParent;
    public Text headerText;
    public Button toggleButton;
    public Button closeButton;
    public GameObject itemTemplate;

    [Header("Settings")]
    public bool createUIOnStart = false;
    public bool autoBindSceneUI = true;
    public bool hidePanelOnStart = true;

    private readonly Color panelColor = new Color(0.67f, 0.64f, 0.64f, 0.55f);
    private readonly Color darkTextColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    private readonly Color blueButtonColor = new Color(0.121f, 0.542f, 0.736f, 1f);
    private readonly Color unlockedGreenColor = new Color(0.50f, 0.82f, 0.50f, 0.78f);
    private readonly Color lockedGrayColor = new Color(0.75f, 0.75f, 0.75f, 0.62f);

    private bool listenersAdded;

    void Start()
    {
        if (autoBindSceneUI)
            BindSceneUIIfNeeded();

        if (createUIOnStart && panelRoot == null)
            CreateFallbackPanelUI();

        AddButtonListeners();

        if (itemTemplate != null)
            itemTemplate.SetActive(false);

        if (panelRoot != null && hidePanelOnStart)
            panelRoot.SetActive(false);

        Refresh();
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
        {
            if (autoBindSceneUI)
                BindSceneUIIfNeeded();

            if (panelRoot == null && createUIOnStart)
                CreateFallbackPanelUI();
        }

        if (panelRoot == null)
            return;

        panelRoot.SetActive(!panelRoot.activeSelf);
        Refresh();
    }

    public void HidePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        AchievementManager manager = AchievementManager.Instance;
        if (manager == null)
            return;

        if (autoBindSceneUI)
            BindSceneUIIfNeeded();

        if (contentParent == null)
            return;

        if (headerText != null)
            headerText.text = "Достижения " + manager.GetUnlockedCount() + "/" + manager.GetTotalCount();

        ClearAchievementRows();

        foreach (AchievementInfo achievement in manager.Achievements)
            CreateAchievementRow(achievement);
    }

    void BindSceneUIIfNeeded()
    {
        if (panelRoot == null)
            panelRoot = FindSceneGameObject("AchievementsPanel");

        if (contentParent == null)
        {
            GameObject content = FindSceneGameObject("Content", panelRoot != null ? panelRoot.transform : null);
            if (content != null)
                contentParent = content.transform;
        }

        if (itemTemplate == null)
            itemTemplate = FindSceneGameObject("AchievementItemTemplate", panelRoot != null ? panelRoot.transform : null);

        if (headerText == null && panelRoot != null)
        {
            Transform title = FindChildRecursive(panelRoot.transform, "Title");
            if (title != null)
                headerText = title.GetComponent<Text>();
        }

        if (closeButton == null && panelRoot != null)
        {
            Transform close = FindChildRecursive(panelRoot.transform, "CloseButton");
            if (close != null)
                closeButton = close.GetComponent<Button>();
        }

        if (toggleButton == null)
        {
            GameObject buttonObject = FindSceneGameObject("AchievementsButton");
            if (buttonObject == null)
                buttonObject = FindSceneGameObject("AchievementButton");

            if (buttonObject != null)
                toggleButton = buttonObject.GetComponent<Button>();
        }
    }

    void AddButtonListeners()
    {
        if (listenersAdded)
            return;

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(HidePanel);

        listenersAdded = true;
    }

    void ClearAchievementRows()
    {
        if (contentParent == null)
            return;

        List<GameObject> objectsToDestroy = new List<GameObject>();

        foreach (Transform child in contentParent)
        {
            if (itemTemplate != null && child.gameObject == itemTemplate)
                continue;

            // В редакторе эти строки нужны как предпросмотр дизайна.
            // В игре они удаляются и заменяются настоящими данными из AchievementManager.
            if (child.name.StartsWith("AchievementPreview_") || child.name.StartsWith("AchievementItem_"))
                objectsToDestroy.Add(child.gameObject);
        }

        foreach (GameObject obj in objectsToDestroy)
            Destroy(obj);
    }

    void CreateAchievementRow(AchievementInfo achievement)
    {
        GameObject row;

        if (itemTemplate != null)
        {
            row = Instantiate(itemTemplate, contentParent);
            row.name = "AchievementItem_" + achievement.id;
            row.SetActive(true);
            FillExistingRow(row, achievement);
            return;
        }

        row = CreateRuntimeRow(contentParent, "AchievementItem_" + achievement.id);
        FillExistingRow(row, achievement);
    }

    GameObject CreateRuntimeRow(Transform parent, string rowName)
    {
        GameObject row = new GameObject(rowName, typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        row.transform.SetParent(parent, false);

        LayoutElement layoutElement = row.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 82f;
        layoutElement.minHeight = 82f;

        RectTransform rowRect = row.GetComponent<RectTransform>();
        rowRect.sizeDelta = new Vector2(0f, 82f);

        CreateText("TitleText", row.transform, new Vector2(18f, -9f), new Vector2(560f, 26f), 17, FontStyle.Bold);
        CreateText("DescriptionText", row.transform, new Vector2(18f, -38f), new Vector2(680f, 34f), 13, FontStyle.Normal);

        Text status = CreateText("StatusText", row.transform, new Vector2(-150f, -24f), new Vector2(132f, 28f), 15, FontStyle.Bold);
        RectTransform statusRect = status.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(1f, 1f);
        statusRect.anchorMax = new Vector2(1f, 1f);
        statusRect.pivot = new Vector2(1f, 1f);
        status.alignment = TextAnchor.MiddleRight;

        return row;
    }

    void FillExistingRow(GameObject row, AchievementInfo achievement)
    {
        Image image = row.GetComponent<Image>();
        if (image != null)
            image.color = achievement.unlocked ? unlockedGreenColor : lockedGrayColor;

        LayoutElement layoutElement = row.GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = row.AddComponent<LayoutElement>();

        layoutElement.preferredHeight = 82f;
        layoutElement.minHeight = 82f;

        Text titleText = FindText(row.transform, "TitleText");
        Text descriptionText = FindText(row.transform, "DescriptionText");
        Text statusText = FindText(row.transform, "StatusText");

        Text[] allTexts = row.GetComponentsInChildren<Text>(true);
        if (titleText == null && allTexts.Length > 0)
            titleText = allTexts[0];
        if (descriptionText == null && allTexts.Length > 1)
            descriptionText = allTexts[1];
        if (statusText == null && allTexts.Length > 2)
            statusText = allTexts[2];

        if (titleText == null)
            titleText = CreateText("TitleText", row.transform, new Vector2(18f, -9f), new Vector2(560f, 26f), 17, FontStyle.Bold);
        if (descriptionText == null)
            descriptionText = CreateText("DescriptionText", row.transform, new Vector2(18f, -38f), new Vector2(680f, 34f), 13, FontStyle.Normal);
        if (statusText == null)
            statusText = CreateText("StatusText", row.transform, new Vector2(-150f, -24f), new Vector2(132f, 28f), 15, FontStyle.Bold);

        string status = achievement.unlocked ? "Открыто" : "Закрыто";

        titleText.text = achievement.title;
        titleText.color = achievement.unlocked ? new Color(0.05f, 0.30f, 0.05f, 1f) : darkTextColor;

        descriptionText.text = achievement.description;
        descriptionText.color = darkTextColor;

        statusText.text = status;
        statusText.color = achievement.unlocked ? new Color(0.05f, 0.30f, 0.05f, 1f) : darkTextColor;
        statusText.alignment = TextAnchor.MiddleRight;
    }

    Text FindText(Transform root, string childName)
    {
        Transform child = FindChildRecursive(root, childName);
        if (child == null)
            return null;

        return child.GetComponent<Text>();
    }

    void CreateFallbackPanelUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return;

        if (toggleButton == null)
            toggleButton = CreateButton(canvas.transform, "AchievementsButton", "Достижения", new Vector2(-25f, 28f), new Vector2(160f, 44f), false);

        panelRoot = new GameObject("AchievementsPanel", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(920f, 610f);

        panelRoot.GetComponent<Image>().color = panelColor;

        headerText = CreateText("Title", panelRoot.transform, new Vector2(35f, -24f), new Vector2(650f, 44f), 30, FontStyle.Normal);
        headerText.alignment = TextAnchor.MiddleLeft;
        closeButton = CreateButton(panelRoot.transform, "CloseButton", "X", new Vector2(-24f, -20f), new Vector2(48f, 44f), true);

        GameObject scrollObject = new GameObject("AchievementsScrollView", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(panelRoot.transform, false);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(35f, 35f);
        scrollRectTransform.offsetMax = new Vector2(-35f, -95f);
        scrollObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.18f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = new Vector2(-24f, 0f);
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.03f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 10;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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

        itemTemplate = CreateRuntimeRow(content.transform, "AchievementItemTemplate");
        itemTemplate.SetActive(false);
        contentParent = content.transform;
    }

    GameObject FindSceneGameObject(string objectName, Transform root = null)
    {
        if (root != null)
        {
            Transform child = FindChildRecursive(root, objectName);
            if (child != null)
                return child.gameObject;
        }

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in objects)
        {
            if (obj.name != objectName)
                continue;

            if (!obj.scene.IsValid() || !obj.scene.isLoaded)
                continue;

            return obj;
        }

        return null;
    }

    Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null)
            return null;

        if (root.name == childName)
            return root;

        foreach (Transform child in root)
        {
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
                return result;
        }

        return null;
    }

    Scrollbar CreateVerticalScrollbar(Transform parent)
    {
        GameObject scrollbarObject = new GameObject("Scrollbar Vertical", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        scrollbarObject.transform.SetParent(parent, false);

        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = new Vector2(1f, 1f);
        scrollbarRect.pivot = new Vector2(1f, 1f);
        scrollbarRect.offsetMin = new Vector2(-20f, 0f);
        scrollbarRect.offsetMax = Vector2.zero;

        Image scrollbarBackground = scrollbarObject.GetComponent<Image>();
        scrollbarBackground.color = new Color(1f, 1f, 1f, 0.22f);

        GameObject slidingArea = new GameObject("Sliding Area", typeof(RectTransform));
        slidingArea.transform.SetParent(scrollbarObject.transform, false);
        RectTransform slidingRect = slidingArea.GetComponent<RectTransform>();
        slidingRect.anchorMin = Vector2.zero;
        slidingRect.anchorMax = Vector2.one;
        slidingRect.offsetMin = new Vector2(2f, 2f);
        slidingRect.offsetMax = new Vector2(-2f, -2f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(slidingArea.transform, false);
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
        scrollbar.size = 0.3f;

        return scrollbar;
    }

    Button CreateButton(Transform parent, string name, string label, Vector2 position, Vector2 size, bool topRight)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);

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
        image.color = topRight ? Color.white : blueButtonColor;

        Text text = CreateText("Text", obj.transform, Vector2.zero, size, 16, FontStyle.Normal);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        text.color = topRight ? darkTextColor : Color.white;

        return obj.GetComponent<Button>();
    }

    Text CreateText(string name, Transform parent, Vector2 position, Vector2 size, int fontSize, FontStyle style)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);

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
        text.color = darkTextColor;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;

        return text;
    }
}
