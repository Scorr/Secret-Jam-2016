using System.Collections;
using UnityEngine;

/// <summary>
/// Boss damage point.
/// </summary>
public class DamagePoint : MonoBehaviour
{

    private TankBoss _boss;
    private SpriteRenderer _spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        _boss = GetComponentInParent<TankBoss>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = _spriteRenderer.color;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Bullet")
        {
            Destroy(collision.gameObject);
            _boss.TakeDamage(1f);
            
            StopAllCoroutines();
            _spriteRenderer.color = originalColor;

            StartCoroutine(FlashColor());
        }
    }

    private IEnumerator FlashColor()
    {
        _spriteRenderer.color = Color.red;

        // Flash for 5 frames.
        for (int i = 0; i < 5; i++)
        {
            yield return null;
        }

        _spriteRenderer.color = originalColor;
    }
}
