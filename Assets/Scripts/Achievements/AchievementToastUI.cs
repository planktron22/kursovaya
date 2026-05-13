using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Всплывающее уведомление о достижении. Может работать с готовыми UI-элементами
/// или создать простое окно автоматически в стиле текущих панелей проекта.
/// </summary>
public class AchievementToastUI : MonoBehaviour
{
    [Header("Optional ready UI")]
    public GameObject toastRoot;
    public Text titleText;
    public Text descriptionText;

    [Header("Settings")]
    public bool createUIOnStart = true;
    public float showSeconds = 3f;

    private Coroutine showRoutine;

    void Start()
    {
        if (createUIOnStart && toastRoot == null)
            CreateToastUI();

        if (toastRoot != null)
            toastRoot.SetActive(false);
    }

    public void Show(AchievementInfo achievement)
    {
        if (achievement == null)
            return;

        if (toastRoot == null)
            CreateToastUI();

        if (titleText != null)
            titleText.text = "Достижение открыто: " + achievement.title;

        if (descriptionText != null)
            descriptionText.text = achievement.description;

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        toastRoot.SetActive(true);
        yield return new WaitForSeconds(showSeconds);
        toastRoot.SetActive(false);
    }

    void CreateToastUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return;

        toastRoot = new GameObject("AchievementToast", typeof(RectTransform), typeof(Image));
        toastRoot.transform.SetParent(canvas.transform, false);

        RectTransform rootRect = toastRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 1f);
        rootRect.anchoredPosition = new Vector2(-20f, -20f);
        rootRect.sizeDelta = new Vector2(360f, 95f);

        Image background = toastRoot.GetComponent<Image>();
        background.color = new Color(0.67f, 0.64f, 0.64f, 0.92f);

        titleText = CreateText("Title", toastRoot.transform, new Vector2(15f, -14f), new Vector2(330f, 30f), 16, FontStyle.Bold);
        descriptionText = CreateText("Description", toastRoot.transform, new Vector2(15f, -48f), new Vector2(330f, 40f), 14, FontStyle.Normal);
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
        text.color = new Color(0.196f, 0.196f, 0.196f, 1f);
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        return text;
    }
}
