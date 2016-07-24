using UnityEngine;

public class BossTurret : MonoBehaviour
{
    private Color originalColor;
    private int _shootLeanTweenId;
    private GameObject _laserGameObject;

    private static int _neo; // worst code ever to make sure sound effect only plays once
    private bool _chosenOne; // ¯\_(ツ)_/¯ it's a gamejam

    private void Awake()
    {
        _neo++;
        if (_neo % 2 == 0)
        {
            _chosenOne = true;
        }

        _laserGameObject = transform.Find("Laser").gameObject;
        originalColor = GetComponent<SpriteRenderer>().color;
        TankBoss.BossRekt += () =>
        {
            LeanTween.cancel(_shootLeanTweenId);
            LeanTween.color(gameObject, originalColor, 0f);
        };
        Shoot();
    }

    public void Shoot()
    {
        _shootLeanTweenId = LeanTween.color(gameObject, Color.red, 3.5f)
            .setEase(LeanTweenType.easeInCubic)
            .setOnComplete(() =>
            {
                _laserGameObject.SetActive(true);
                if (_chosenOne)
                    SoundManager.Instance.PlaySound("laser", 0.8f);
                LeanTween.alpha(_laserGameObject, 0.1f, 0f);
                LeanTween.alpha(_laserGameObject, 1f, 0.75f)
                    .setEase(LeanTweenType.easeOutExpo)
                    .setLoopPingPong(1)
                    .setOnComplete(() => _laserGameObject.SetActive(false));
            })
            .setOnCompleteOnRepeat(true)
            .setRepeat(-1).id;
    }
}