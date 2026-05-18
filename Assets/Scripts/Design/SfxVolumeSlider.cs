using UnityEngine;
using UnityEngine.UI;

public class SfxVolumeSlider : MonoBehaviour
{
    public Slider slider;

    void OnEnable()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider == null)
        {
            Debug.LogError("SfxVolumeSlider: Slider не найден");
            return;
        }

        slider.SetValueWithoutNotify(SoundSettings.SfxVolume);

        slider.onValueChanged.RemoveListener(SetSfxVolume);
        slider.onValueChanged.AddListener(SetSfxVolume);
    }

    void OnDisable()
    {
        if (slider != null)
            slider.onValueChanged.RemoveListener(SetSfxVolume);
    }

    void SetSfxVolume(float value)
    {
        SoundSettings.SfxVolume = value;
    }
}