using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public bool Shooting { get; set; }
    public IObservable<Vector2> Movement { get; private set; }
    public IObservable<Vector2> Rotation { get; private set; }
    public ReactiveProperty<float> DashCooldown;
    public float MaxDashCooldown { get; set; }
    private float _moveSpeed = 0.1f;
    [SerializeField] private Animator _legsAnimator;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _shootTransformRight;
    [SerializeField] private Transform _shooTransformLeft;
    [SerializeField] private Material _spriteMaterial;
    [SerializeField] private Material _distortionMaterial;
    [SerializeField] private PowerupSpawner _powerupSpawner;
    [SerializeField] private GameObject _boostedBulletPrefab;
    private Rigidbody2D _rigidbody;
    private bool _knockback; // Add knockback force in FixedUpdate?

    private float _shootCooldown;
    private int _shotsFired; // Used for determining which gun to fire from.
    private bool _gameOver;
    private bool _invulnerable;
    private bool _boosted; // picked up a powerup?

    private void Awake()
    {
        DashCooldown = new ReactiveProperty<float>(0f);
        MaxDashCooldown = 2f;
        Shooting = true;
        _rigidbody = GetComponent<Rigidbody2D>();

        Movement = this.UpdateAsObservable()
          .Select(_ =>
          {
              var x = Input.GetAxisRaw("Horizontal");
              var y = Input.GetAxisRaw("Vertical");
              return new Vector2(x, y);
          });

        var mouseRotation = this.UpdateAsObservable()
            .Where(_ => Math.Abs(Input.GetAxis("Mouse X")) > 0.1f || Math.Abs(Input.GetAxis("Mouse Y")) > 0.1f)
            .Select(_ =>
            {
                Vector2 cursorInWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 retVal = (cursorInWorldPos - (Vector2)transform.position).normalized;
                retVal.y = -retVal.y; // Invert y pos due to character rotation.
                return retVal;
            });

        var stickRotation = this.UpdateAsObservable()
            .Select(_ =>
            {
                var x = Input.GetAxis("RightH");
                var y = Input.GetAxis("RightV");
                return new Vector2(x, y);
            });

        Rotation = mouseRotation.Merge(stickRotation);

        Movement.Subscribe(input =>
        {
            _rigidbody.velocity = input * 2f;
            _legsAnimator.SetFloat("movement", Mathf.Clamp01(Mathf.Abs(input.x) + Mathf.Abs(input.y)));
        }).AddTo(this);

        Rotation.Where(input => input != Vector2.zero).Subscribe(input =>
        {
            if (_gameOver) return;
            var angle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        });
    }

    private void Update()
    {
        if (_shootCooldown > 0)
        {
            _shootCooldown -= Time.deltaTime;
        }
        else
        {
            _shootCooldown = 0;
            Shoot();
        }

        if (DashCooldown.Value > 0)
            DashCooldown.Value -= Time.deltaTime;
        else
            DashCooldown.Value = 0;

        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Fire2"))
            Dash();
    }

    private void FixedUpdate()
    {
        if (_knockback)
        {
            _rigidbody.AddForce(transform.up * 3f, ForceMode2D.Impulse);
            _knockback = false;
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Powerup")
        {
            StartCoroutine(_powerupSpawner.SpawnPowerup(collision.transform.position.x > 0f));
            _boosted = true;
            Invoke("Unboost", 3f);
            Destroy(collision.gameObject);
        }

        if (_invulnerable)
            return;

        if (collision.tag == "BossBullet")
        {
            Destroy(collision.gameObject);
            GameOver();
        }
        else if (collision.tag == "BossLaser")
        {
            GameOver();
        }
    }

    /// <summary>
    /// Use in conjunction with Invoke to disable powerup after a delay.
    /// </summary>
    private void Unboost()
    {
        _boosted = false;
    }

    private void GameOver()
    {
        _gameOver = true;
        Time.timeScale = 0;
        SoundManager.Instance.PlaySound("death", 0.5f);
        LeanTween.alpha(gameObject, 0f, 0.1f).setLoopPingPong().setUseEstimatedTime(true);
        StartCoroutine(GameOverDelay());
    }

    private IEnumerator GameOverDelay()
    {
        yield return new WaitForSecondsRealtime(1.75f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(1);
    }

    private void Shoot()
    {
        if (_shootCooldown <= 0 && Shooting)
        {
            if (_boosted)
            {
                if (_shotsFired % 2 == 0)
                    Instantiate(_boostedBulletPrefab, _shootTransformRight.position,
                        Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 90)));
                else
                    Instantiate(_boostedBulletPrefab, _shooTransformLeft.position,
                        Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 90)));
                Screenshake.Instance.DoShake(0.1f, 0.03f);
                _shootCooldown = 0.15f;
            }
            else
            {
                if (_shotsFired%2 == 0)
                    Instantiate(_bulletPrefab, _shootTransformRight.position,
                        Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 90)));
                else
                    Instantiate(_bulletPrefab, _shooTransformLeft.position,
                        Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 90)));
                _shootCooldown = 0.25f;
            }
            _shotsFired++;
            _knockback = true;
            
            SoundManager.Instance.PlaySound("playershoot", 0.4f);
        }
    }

    private void Dash()
    {
        if (DashCooldown.Value > 0)
            return;

        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.material = _distortionMaterial;
            var mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            mpb.SetFloat("_Outline", 1f);
            mpb.SetColor("_OutlineColor", Color.white);
            mpb.SetFloat("_FadeoutValue", 0.5f);
            renderer.SetPropertyBlock(mpb);
        }

        LeanTween.move(gameObject, transform.position + -transform.up * 1.5f, 0.3f).setOnComplete(() =>
        {
            foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.material = _spriteMaterial;
            }
            _invulnerable = false;
        });

        _invulnerable = true;
        DashCooldown.Value = MaxDashCooldown;
    }

}
