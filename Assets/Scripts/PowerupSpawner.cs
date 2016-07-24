using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _powerupPrefab;
    [SerializeField] private float _cooldownBetweenPickups;

    private void Start()
    {
        StartCoroutine(SpawnPowerup());
    }

    public IEnumerator SpawnPowerup()
    {
        float minX = -5.7f;
        float maxX = 5.7f;
        float minY = -2.6f; // Bottom of scene
        float maxY = 2.6f;

        yield return new WaitForSeconds(_cooldownBetweenPickups);

        var randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        Instantiate(_powerupPrefab, randomPosition, Quaternion.identity);
    }
}
