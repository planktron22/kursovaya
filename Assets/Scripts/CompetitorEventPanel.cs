using UnityEngine;
using UnityEngine.UI;


public class CompetitorEventPanel : MonoBehaviour
{
    [Header("Optional ready UI (оставь пустым — создастся автоматически)")]
    public GameObject panelRoot;
    public Text       titleText;
    public Text       messageText;
    public Image      headerBackground;
    public Button     closeButton;

  
    private static readonly Color ColPanelBg        = new Color(0.13f, 0.13f, 0.16f, 0.97f);
    private static readonly Color ColHeaderSabotage = new Color(0.80f, 0.18f, 0.18f, 1f);
    private static readonly Color ColHeaderHelp     = new Color(0.18f, 0.68f, 0.28f, 1f);
    private static readonly Color ColTextWhite      = new Color(0.95f, 0.95f, 0.95f, 1f);
    private static readonly Color ColTextGray       = new Color(0.80f, 0.80f, 0.80f, 1f);
    private static readonly Color ColDivider        = new Color(1f,    1f,    1f,    0.10f);
    private static readonly Color ColCloseBg        = new Color(0f,    0f,    0f,    0.20f);



    void Start()
    {
        if (panelRoot == null)
            BuildUI();

        panelRoot.SetActive(false);
    }


    public void Show(string competitorName, string message, bool isSabotage)
    {
        if (panelRoot == null) BuildUI();

        if (titleText != null)
            titleText.text = isSabotage
                ? $"⚔  {competitorName}"
                : $"✦  {competitorName}";

        if (messageText != null)
            messageText.text = message;

        if (headerBackground != null)
            headerBackground.color = isSabotage ? ColHeaderSabotage : ColHeaderHelp;

        panelRoot.SetActive(true);
    }

    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        UIManager ui = FindObjectOfType<UIManager>();
        if (ui != null)
            ui.isPanelOpen = false;
    }



    void BuildUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[CompetitorEventPanel] Canvas не найден — UI не создан.");
            return;
        }

        // ── Корневая панель ──────────────────────────────────────────────────
        panelRoot = MakeGO("CompetitorEventPanel", canvas.transform);
        panelRoot.transform.SetAsLastSibling();

        RectTransform rootRect = panelRoot.AddComponent<RectTransform>();
        rootRect.anchorMin        = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax        = new Vector2(0.5f, 0.5f);
        rootRect.pivot            = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = new Vector2(0f, 60f);
        rootRect.sizeDelta        = new Vector2(430f, 158f);

        panelRoot.AddComponent<Image>().color = ColPanelBg;

        // ── Шапка ────────────────────────────────────────────────────────────
        GameObject header = MakeGO("Header", panelRoot.transform);
        RectTransform hRect = header.AddComponent<RectTransform>();
        hRect.anchorMin        = new Vector2(0f, 1f);
        hRect.anchorMax        = new Vector2(1f, 1f);
        hRect.pivot            = new Vector2(0.5f, 1f);
        hRect.anchoredPosition = Vector2.zero;
        hRect.sizeDelta        = new Vector2(0f, 46f);

        headerBackground       = header.AddComponent<Image>();
        headerBackground.color = ColHeaderSabotage; // перезапишется в Show()

        // Имя конкурента
        titleText = MakeText("TitleText", header.transform,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(-22f, 0f), Vector2.zero,
            17, FontStyle.Bold, ColTextWhite, TextAnchor.MiddleCenter);

        // Кнопка X — правый край шапки
        closeButton = MakeCloseButton(header.transform);
        closeButton.onClick.AddListener(Hide);

        // ── Разделитель ──────────────────────────────────────────────────────
        GameObject div = MakeGO("Divider", panelRoot.transform);
        RectTransform dRect = div.AddComponent<RectTransform>();
        dRect.anchorMin        = new Vector2(0f, 1f);
        dRect.anchorMax        = new Vector2(1f, 1f);
        dRect.pivot            = new Vector2(0.5f, 1f);
        dRect.anchoredPosition = new Vector2(0f, -46f);
        dRect.sizeDelta        = new Vector2(0f, 1f);
        div.AddComponent<Image>().color = ColDivider;

        // ── Текст события ────────────────────────────────────────────────────
        messageText = MakeText("MessageText", panelRoot.transform,
            new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
            new Vector2(0f, -24f), new Vector2(-36f, -56f),
            14, FontStyle.Normal, ColTextGray, TextAnchor.MiddleCenter);
        messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
        messageText.verticalOverflow   = VerticalWrapMode.Overflow;

        Debug.Log("[CompetitorEventPanel] UI создан автоматически.");
    }



    static GameObject MakeGO(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    static Text MakeText(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 pos, Vector2 size,
        int fontSize, FontStyle style, Color color, TextAnchor alignment)
    {
        GameObject go = MakeGO(name, parent);

        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin        = anchorMin;
        r.anchorMax        = anchorMax;
        r.pivot            = pivot;
        r.anchoredPosition = pos;
        r.sizeDelta        = size;

        Text t = go.AddComponent<Text>();
        t.font               = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize           = fontSize;
        t.fontStyle          = style;
        t.color              = color;
        t.alignment          = alignment;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow   = VerticalWrapMode.Overflow;

        return t;
    }

    static Button MakeCloseButton(Transform parent)
    {
        GameObject go = MakeGO("CloseButton", parent);

        RectTransform r = go.AddComponent<RectTransform>();
        r.anchorMin        = new Vector2(1f, 0f);
        r.anchorMax        = new Vector2(1f, 1f);
        r.pivot            = new Vector2(1f, 0.5f);
        r.anchoredPosition = Vector2.zero;
        r.sizeDelta        = new Vector2(46f, 0f);

        go.AddComponent<Image>().color = ColCloseBg;

        Text label = MakeText("Label", go.transform,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero,
            18, FontStyle.Bold, new Color(0.95f, 0.95f, 0.95f, 1f),
            TextAnchor.MiddleCenter);
        label.text = "✕";

        return go.AddComponent<Button>();
    }
}
