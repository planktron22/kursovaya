using UnityEngine;
using System.Globalization;

public class PeriodPopupSpawner : MonoBehaviour
{
    public GameObject popupPrefab;
    public Canvas targetCanvas;
    public Transform popupPoint;

    public Color positiveColor = new Color(0.2f, 0.8f, 0.3f);
    public Color negativeColor = new Color(0.9f, 0.2f, 0.2f);
    public Color zeroColor = Color.white;

    private CultureInfo culture = new CultureInfo("ru-RU");

    public void ShowPeriodPopup(int value)
    {
        if (popupPrefab == null || targetCanvas == null || popupPoint == null)
        {
            Debug.LogError("Не назначены ссылки для PeriodPopupSpawner");
            return;
        }

        GameObject obj = Instantiate(popupPrefab, targetCanvas.transform);

        RectTransform rect = obj.GetComponent<RectTransform>();

        Vector3 screenPos = Camera.main.WorldToScreenPoint(popupPoint.position);

        rect.position = screenPos;

        string sign = value > 0 ? "+" : "";

        Color textColor;

        if (value > 0)
            textColor = positiveColor;
        else if (value < 0)
            textColor = negativeColor;
        else
            textColor = zeroColor;

        PeriodPopupUI popup = obj.GetComponent<PeriodPopupUI>();
        popup.Setup(sign + value.ToString("N0", culture) + " р.", textColor);
    }
}