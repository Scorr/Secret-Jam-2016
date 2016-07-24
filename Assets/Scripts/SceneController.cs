using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static event Action OnSceneChange; // Called before a scene changes.
    [SerializeField] private Animator _titleScreenFade;

    public void LoadLevel(int level)
    {
        if (OnSceneChange != null)
            OnSceneChange();
        
        _titleScreenFade.Play("TitleScreenFade2");
        SoundManager.Instance.PlaySound("startgame", 0.8f);

        StartCoroutine(LoadScene(level, 1f));
    }

    private IEnumerator LoadScene(int level, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(level);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}