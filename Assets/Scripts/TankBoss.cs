using System;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankBoss : MonoBehaviour
{

    public float MaxHealth = 100f;
    public ReactiveProperty<float> Health { get; private set; }
    public static event Action BossRekt;

    private Rigidbody2D _rigidbody;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _cannon;
    [SerializeField] private GameObject _base; // Tank body.
    [SerializeField] private Transform _player;
    [SerializeField] private float _cannonRotationSpeed = 90f;
    [SerializeField] private float _baseRotationSpeed = 25f;
    [SerializeField] private Transform _shootTransform; // The position where bullets get instantiated.
    [SerializeField] private GameObject _bulletPrefab;
    private float _cooldown;
    private int _shotsFired = -1; // Don't start at 0 to give the player a second of breathing when game starts.

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        Health = new FloatReactiveProperty(MaxHealth);
    }

    private void Update()
    {
        _rigidbody.velocity = Vector2.zero; // Reset velocity.
        if (Health.Value > 0)
        {
            RotateCannon();
            RotateBody();
            MoveForward();

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
    }

    private void RotateCannon()
    {
        Vector3 vectorToTarget = _player.position - _cannon.transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        Quaternion newRotation = Quaternion.RotateTowards(_cannon.transform.rotation, q, Time.deltaTime * _cannonRotationSpeed);
        newRotation.eulerAngles = new Vector3(0f, 0f, newRotation.eulerAngles.z);
        _cannon.transform.rotation = newRotation;
    }

    private void RotateBody()
    {
        Vector3 vectorToTarget = _player.position - _base.transform.position;
        float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        Quaternion newRotation = Quaternion.RotateTowards(_base.transform.rotation, q, Time.deltaTime * _baseRotationSpeed);
        newRotation.eulerAngles = new Vector3(0f, 0f, newRotation.eulerAngles.z);
        _base.transform.rotation = newRotation;
    }

    private void MoveForward()
    {
        _rigidbody.velocity = _base.transform.right * 0.25f;
    }

    private void Shoot()
    {
        _shotsFired++;
        if (_shotsFired%4 == 0) // Wait after every third shot.
        {
            _cooldown = 1.75f;
            return;
        }
        else if (_shotsFired%5 == 0)
        {
            Instantiate(_bulletPrefab, _shootTransform.position,
                _cannon.transform.rotation*Quaternion.Euler(0f, 0f, -40f));
            Instantiate(_bulletPrefab, _shootTransform.position,
                _cannon.transform.rotation*Quaternion.Euler(0f, 0f, -20f));
            Instantiate(_bulletPrefab, _shootTransform.position,
                _cannon.transform.rotation*Quaternion.Euler(0f, 0f, 20f));
            Instantiate(_bulletPrefab, _shootTransform.position,
                _cannon.transform.rotation * Quaternion.Euler(0f, 0f, 40f));
            _cooldown = 1.5f;
        }
        else
        {
            Instantiate(_bulletPrefab, _shootTransform.position, _cannon.transform.rotation);
            Instantiate(_bulletPrefab, _shootTransform.position,
                _cannon.transform.rotation*Quaternion.Euler(0f, 0f, 30f));
            Instantiate(_bulletPrefab, _shootTransform.position,
                _cannon.transform.rotation*Quaternion.Euler(0f, 0f, -30f));
            _cooldown = 1f;
        }
        
        SoundManager.Instance.PlaySound("bossshoot", 0.3f);
    }

    public void TakeDamage(float damage)
    {
        if (Health.Value - damage > 0)
        {
            Health.Value -= damage;
            SoundManager.Instance.PlaySound("hit");
        }
        else
        {
            Health.Value = 0;
            Die();
        }
    }

    public void Die()
    {
        float maxExplosionStart = 2f; // The delay the last explosion can spawn.
        for (int i = 0; i < 80; i++)
        {
            Invoke("InstantiateExplosion", Random.Range(0f, maxExplosionStart));
        }
        Screenshake.Instance.DoShake(0.05f, maxExplosionStart + 0.5f); // Explosion animation lasts about 0.5 seconds
        SoundManager.Instance.PlaySound("boss-death", 0.5f);
        _player.GetComponent<PlayerController>().Shooting = false;
        var bullets = GameObject.FindGameObjectsWithTag("BossBullet");
        for (int i = 0; i < bullets.Length; i++)
        {
            Destroy(bullets[i].gameObject);
        }
        Destroy(_cannon);

        if (BossRekt != null)
            BossRekt.Invoke();

        Time.timeScale = 0.5f;
    }

    public void InstantiateExplosion()
    {
        var explosion = (GameObject)Instantiate(_explosionPrefab, (Vector2)transform.position + Random.insideUnitCircle, Quaternion.identity);
        float minScale = 1.5f;
        float maxScale = 3.5f;
        float randomScale = Random.Range(minScale, maxScale);
        explosion.transform.localScale = new Vector3(randomScale, randomScale, 1f);
    }
}
