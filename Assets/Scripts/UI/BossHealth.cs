using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class BossHealth : MonoBehaviour
    {

        [SerializeField] private TankBoss _tankBoss;
        [SerializeField] private Image _healthBar;
        [SerializeField] private GameObject _victoryText;
        [SerializeField] private GameObject _victorySubText;

        private void Awake()
        {
            _tankBoss.Health.Subscribe(value =>
            {
                _healthBar.fillAmount = value/_tankBoss.MaxHealth;
                if (value <= 0)
                {
                    StartCoroutine(PlayDelayed());
                }
            }).AddTo(this);
        }

        private IEnumerator PlayDelayed()
        {
            yield return new WaitForSecondsRealtime(2f);
            _victoryText.GetComponent<Animator>().Play("VictoryText");
            yield return new WaitForSeconds(1.5f);
            _victorySubText.GetComponent<Animator>().Play("VictorySubText");
            yield return new WaitForSecondsRealtime(10f);
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
        }
    }
}
