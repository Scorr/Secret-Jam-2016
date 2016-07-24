using System.Collections;
using UnityEngine;

public class SpriteTrail : MonoBehaviour
{
    [SerializeField] private Material _glitchEffect;

    public void StartTrailing()
    {
        InvokeRepeating("SpawnTrail", 0, 0.05f); // replace 0.2f with needed repeatRate
    }

    public void StopTrailing()
    {
        CancelInvoke();
    }

    private void SpawnTrail()
    {
        GameObject trailPart = new GameObject();
        SpriteRenderer trailPartRenderer = trailPart.AddComponent<SpriteRenderer>();
        trailPartRenderer.sprite = GetComponent<SpriteRenderer>().sprite;

        trailPartRenderer.material = _glitchEffect;
        var mpb = new MaterialPropertyBlock();
        trailPartRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat("_Outline", 1f);
        mpb.SetColor("_OutlineColor", Color.white);
        mpb.SetFloat("_FadeoutValue", 0.8f);
        trailPartRenderer.SetPropertyBlock(mpb);

        trailPart.transform.position = transform.position;
        trailPart.transform.rotation = transform.rotation;
        Destroy(trailPart, 0.15f); // replace 0.5f with needed lifeTime

        StartCoroutine("FadeTrailPart", trailPartRenderer);
    }

    private IEnumerator FadeTrailPart(SpriteRenderer trailPartRenderer)
    {
        Color color = trailPartRenderer.color;
        color.a -= 0.5f; // replace 0.5f with needed alpha decrement
        trailPartRenderer.color = color;

        yield return new WaitForEndOfFrame();
    }
}
