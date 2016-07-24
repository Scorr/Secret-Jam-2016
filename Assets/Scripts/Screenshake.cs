using System.Collections;
using UnityEngine;

public class Screenshake : Singleton<Screenshake>
{

    private Vector3 originalCameraPosition;

    private void Awake()
    {
        originalCameraPosition = Camera.main.transform.position;
    }
    
    public void DoShake(float intensity, float duration)
    {
        Camera.main.transform.position = originalCameraPosition;
        StopAllCoroutines();
        StartCoroutine(Shake(intensity, duration));
    }

    private IEnumerator Shake(float intensity, float duration)
    {
        float startTime = Time.time;
        
        while (Time.time - startTime <= duration)
        {
            Vector3 pos = originalCameraPosition;
            float quakeX = Random.Range(-1f, 1f)*intensity;
            float quakeY = Random.Range(-1f, 1f) * intensity;
            pos.x += quakeX;
            pos.y += quakeY;
            Camera.main.transform.position = pos;

            yield return null;
        }
        Camera.main.transform.position = originalCameraPosition;
    }
}
