using System.Collections;
using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _powerupPrefab;
    [SerializeField] private float _cooldownBetweenPickups;

    private void Start()
    {
        StartCoroutine(SpawnPowerup(true));
        StartCoroutine(SpawnPowerup(false));
    }

    public IEnumerator SpawnPowerup(bool leftHalf)
    {
        float minX;
        float maxX;
        if (leftHalf)
        {
            minX = -5.7f;
            maxX = 0f;
        }
        else
        {
            minX = 0f;
            maxX = 5.7f;
        }
        float minY = -2.6f; // Bottom of scene
        float maxY = 2.6f;

        yield return new WaitForSeconds(_cooldownBetweenPickups);

        var randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        Instantiate(_powerupPrefab, randomPosition, Quaternion.identity);
    }
}
