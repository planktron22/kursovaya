using UnityEngine;

public static class SoundSettings
{
    private const string SfxVolumeKey = "SfxVolume";

    public static float SfxVolume
    {
        get
        {
            return PlayerPrefs.GetFloat(SfxVolumeKey, 0.7f);
        }
        set
        {
            float clampedValue = Mathf.Clamp01(value);

            PlayerPrefs.SetFloat(SfxVolumeKey, clampedValue);
            PlayerPrefs.Save();

            Debug.Log("Громкость эффектов: " + clampedValue);
        }
    }
}