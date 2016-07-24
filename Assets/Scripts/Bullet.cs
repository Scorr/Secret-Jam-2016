using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Damage = 1f;

    private void Awake()
    {
        // Set constant moving force.
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.velocity = -transform.right * 5f;

        Invoke("DestroySelf", 3f);
    }

    protected void DestroySelf()
    {
        Destroy(gameObject);
    }
}
