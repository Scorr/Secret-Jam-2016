using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : Singleton<SoundManager>
{
    private static bool _waitingforintro; // stupid exceptional fix.
    private AudioSource _source;
    private AudioClip _introMusic;
    private AudioClip _loopMusic;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            _introMusic = Resources.Load<AudioClip>("intro");
            _source.clip = _introMusic;
            _source.Play();

            if (!_waitingforintro)
            {
                _loopMusic = Resources.Load<AudioClip>("loop");
                StartCoroutine(PlayMusic());
            }
        }
    }

    private IEnumerator PlayMusic()
    {
        _waitingforintro = true;
        yield return new WaitForSecondsRealtime(_introMusic.length);
        _waitingforintro = false;
        _source.clip = _loopMusic;
        _source.loop = true;
        _source.Play();
    }

    public void PlaySound(string name, float volume = 1f)
    {
        _source.PlayOneShot(Resources.Load<AudioClip>(name), volume);
    }
}
