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
    private float _moveSpeed = 0.05f;
    [SerializeField] private Animator _legsAnimator;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _shootTransformRight;
    [SerializeField] private Transform _shooTransformLeft;
    private Rigidbody2D _rigidbody;
    private bool _knockback; // Add knockback force in FixedUpdate?

    private float _cooldown;
    private int _shotsFired; // Used for determining which gun to fire from.
    private bool _gameOver;

    private void Awake()
    {
        Shooting = true;
        _rigidbody = GetComponent<Rigidbody2D>();

        Movement = this.UpdateAsObservable()
          .Select(_ =>
          {
              var x = Input.GetAxis("Horizontal");
              var y = Input.GetAxis("Vertical");
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
        if (_cooldown > 0)
        {
            _cooldown -= Time.deltaTime;
        }
        else
        {
            _cooldown = 0;
            Shoot();
        }
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
        if (_cooldown <= 0 && Shooting)
        {
            if (_shotsFired % 2 == 0)
                Instantiate(_bulletPrefab, _shootTransformRight.position, Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 90)));
            else
                Instantiate(_bulletPrefab, _shooTransformLeft.position, Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, 90)));
            _shotsFired++;
            _cooldown = 0.25f;
            _knockback = true;
            
            SoundManager.Instance.PlaySound("playershoot", 0.4f);
        }
    }

}
