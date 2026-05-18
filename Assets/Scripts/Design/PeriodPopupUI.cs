using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PeriodPopupUI : MonoBehaviour
{
    public Text popupText;
    public float lifeTime = 1.5f;
    public float moveUpDistance = 80f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Setup(string text, Color textColor)
    {
        popupText.text = text;
        popupText.color = textColor;

        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, moveUpDistance);

        float timer = 0f;

        while (timer < lifeTime)
        {
            timer += Time.deltaTime;

            float t = timer / lifeTime;

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        Destroy(gameObject);
    }
}