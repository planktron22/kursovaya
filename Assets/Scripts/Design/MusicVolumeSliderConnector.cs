using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSliderConnector : MonoBehaviour
{
    public Slider slider;

    void OnEnable()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider == null)
        {
            Debug.LogError("MusicVolumeSliderConnector: Slider не найден");
            return;
        }

        if (MusicManager.Instance == null)
        {
            Debug.LogError("MusicManager не найден");
            return;
        }

        slider.SetValueWithoutNotify(MusicManager.Instance.GetVolume());

        slider.onValueChanged.RemoveListener(MusicManager.Instance.SetVolume);
        slider.onValueChanged.AddListener(MusicManager.Instance.SetVolume);
    }
}