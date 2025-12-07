using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("SFX Clips")]
    public AudioSource sfxSource;
    public AudioClip bottleFull;
    public AudioClip bottleClose;
    public AudioClip bottleDown;
    public AudioClip bottleUp;
    public AudioClip winSound;
    public AudioClip loseSound;
    public AudioClip pourSound;

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void PlayUp() => sfxSource.PlayOneShot(bottleUp);
    public void PlayClose() => sfxSource.PlayOneShot(bottleClose);
    public void PlayFull() => sfxSource.PlayOneShot(bottleFull);
    public void PlayDown() => sfxSource.PlayOneShot(bottleDown);
    public void PlayWin() => sfxSource.PlayOneShot(winSound);
    public void PlayLose() => sfxSource.PlayOneShot(loseSound);
    public void PlayPour() => sfxSource.PlayOneShot(pourSound);
}
