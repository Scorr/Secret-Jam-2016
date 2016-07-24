using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : Singleton<SoundManager>
{
    private static bool _waitingforintro; // stupid exceptional fix.
    private AudioSource _source;
    private readonly Dictionary<string, AudioClip> _audioCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            _audioCache.Add("intromusic", Resources.Load<AudioClip>("intro"));
            _source.clip = _audioCache["intromusic"];
            _source.Play();

            if (!_waitingforintro)
            {
                _audioCache.Add("loopmusic", Resources.Load<AudioClip>("loop"));
                StartCoroutine(PlayMusic());
            }
        }
    }

    private IEnumerator PlayMusic()
    {
        _waitingforintro = true;
        yield return new WaitForSecondsRealtime(_audioCache["intromusic"].length);
        _waitingforintro = false;
        _source.clip = _audioCache["loopmusic"];
        _source.loop = true;
        _source.Play();
    }

    public void PlaySound(string name, float volume = 1f)
    {
        AudioClip clip;
        if (_audioCache.TryGetValue(name, out clip))
        {
            _source.PlayOneShot(clip, volume);
        }
        else
        {
            _audioCache.Add(name, Resources.Load<AudioClip>(name));
            _source.PlayOneShot(_audioCache[name], volume);
        }
    }
}
