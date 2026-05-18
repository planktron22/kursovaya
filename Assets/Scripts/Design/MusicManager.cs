using UnityEngine;
using UnityEngine.UI;

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
    }

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.3f);

        musicSource.volume = savedVolume;

        if (!musicSource.isPlaying)
            musicSource.Play();
    }

    public void SetVolume(float value)
    {
        musicSource.volume = value;

        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();

        Debug.Log("Громкость музыки: " + value);
    }

    public float GetVolume()
    {
        return musicSource.volume;
    }
}