using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSliderConnector : MonoBehaviour
{
    public Slider slider;

    void OnEnable()
    {
        StartCoroutine(Connect());
    }

    IEnumerator Connect()
    {
        yield return null;

        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider == null)
        {
            Debug.LogError("MusicVolumeSliderConnector: Slider не найден");
            yield break;
        }

        if (MusicManager.Instance == null)
        {
            Debug.LogError("MusicManager не найден");
            yield break;
        }

        slider.SetValueWithoutNotify(MusicManager.Instance.GetVolume());

        slider.onValueChanged.RemoveListener(MusicManager.Instance.SetVolume);
        slider.onValueChanged.AddListener(MusicManager.Instance.SetVolume);
    }

    void OnDisable()
    {
        if (slider != null && MusicManager.Instance != null)
        {
            slider.onValueChanged.RemoveListener(MusicManager.Instance.SetVolume);
        }
    }
}