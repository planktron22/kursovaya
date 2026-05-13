using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Панель списка достижений. Не требует изменений UIManager:
/// сама создаёт кнопку и панель на Canvas, либо может быть привязана к ручной верстке.
/// </summary>
public class AchievementPanelUI : MonoBehaviour
{
    [Header("Optional ready UI")]
    public GameObject panelRoot;
    public Transform contentParent;
    public Text headerText;
    public Button toggleButton;
    public Button closeButton;

    [Header("Settings")]
    public bool createUIOnStart = true;

    private readonly Color panelColor = new Color(0.67f, 0.64f, 0.64f, 0.392f);
    private readonly Color darkTextColor = new Color(0.196f, 0.196f, 0.196f, 1f);
    private readonly Color blueButtonColor = new Color(0.121f, 0.542f, 0.736f, 1f);

    void Start()
    {
        if (createUIOnStart && panelRoot == null)
            CreatePanelUI();

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(HidePanel);

        if (panelRoot != null)
            panelRoot.SetActive(false);

        Refresh();
    }

    public void TogglePanel()
    {
        if (panelRoot == null)
            CreatePanelUI();

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

        if (manager == null || contentParent == null)
            return;

        if (headerText != null)
        {
            headerText.text = "Достижения " + manager.GetUnlockedCount() + "/" + manager.GetTotalCount();
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (AchievementInfo achievement in manager.Achievements)
            CreateAchievementRow(achievement);
    }

    void CreatePanelUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return;

        if (toggleButton == null)
            toggleButton = CreateButton(canvas.transform, "AchievementsButton", "Достижения", new Vector2(-150f, 45f), new Vector2(130f, 38f), false);

        panelRoot = new GameObject("AchievementsPanel", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(560f, 520f);

        Image panelImage = panelRoot.GetComponent<Image>();
        panelImage.color = panelColor;

        headerText = CreateText("Header", panelRoot.transform, new Vector2(25f, -20f), new Vector2(430f, 35f), 22, FontStyle.Bold);
        closeButton = CreateButton(panelRoot.transform, "CloseButton", "X", new Vector2(-30f, -25f), new Vector2(36f, 32f), true);

        GameObject scrollObject = new GameObject("Scroll View", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollObject.transform.SetParent(panelRoot.transform, false);
        RectTransform scrollRectTransform = scrollObject.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(25f, 25f);
        scrollRectTransform.offsetMax = new Vector2(-25f, -70f);
        scrollObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.392f);

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.05f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 10, 10);
        layout.spacing = 8;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        contentParent = content.transform;
    }

    void CreateAchievementRow(AchievementInfo achievement)
    {
        GameObject row = new GameObject("AchievementItem", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        row.transform.SetParent(contentParent, false);

        LayoutElement layoutElement = row.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;

        Image rowImage = row.GetComponent<Image>();
        rowImage.color = achievement.unlocked
            ? new Color(1f, 1f, 1f, 0.75f)
            : new Color(0.75f, 0.75f, 0.75f, 0.55f);

        string status = achievement.unlocked ? "Открыто" : "Закрыто";
        Text title = CreateText("Title", row.transform, new Vector2(14f, -8f), new Vector2(430f, 25f), 16, FontStyle.Bold);
        title.text = achievement.title + " — " + status;

        Text description = CreateText("Description", row.transform, new Vector2(14f, -35f), new Vector2(490f, 30f), 14, FontStyle.Normal);
        description.text = achievement.description;
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

        Text text = CreateText("Text", obj.transform, Vector2.zero, size, 14, FontStyle.Normal);
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
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = darkTextColor;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
    }
}
