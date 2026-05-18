using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource musicSource;

    private const string MusicVolumeKey = "MusicVolume";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();

        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.3f);
        musicSource.volume = savedVolume;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void SetVolume(float value)
    {
        if (musicSource == null)
            return;

        musicSource.volume = value;

        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    public float GetVolume()
    {
        if (musicSource == null)
            return PlayerPrefs.GetFloat(MusicVolumeKey, 0.3f);

        return musicSource.volume;
    }
}