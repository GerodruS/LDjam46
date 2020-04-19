using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioListener))]
public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer _instance;

    private AudioSource _audioSource;

    public static MusicPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                var gameObject = new GameObject();
                _instance = gameObject.AddComponent<MusicPlayer>();
                gameObject.name = "MusicPlayer";
            }

            return _instance;
        }
    }

    public void PlayMusic(AudioClip audioClip)
    {
        if (!_audioSource.isPlaying ||
            _audioSource.clip != audioClip)
        {
            _audioSource.clip = audioClip;
            _audioSource.Play();
        }
    }

    public void StopMusic()
    {
        _audioSource.Stop();
    }

    public void OneShot(AudioClip audioClip)
    {
        _audioSource.PlayOneShot(audioClip);
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        _audioSource = GetComponent<AudioSource>();
        _audioSource.loop = true;
    }
}
