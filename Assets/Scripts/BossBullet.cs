using UnityEngine;

public class BossBullet : Bullet
{
    private void Awake()
    {
        // Set constant moving force.
        var rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.velocity = transform.right * 5f;
        Invoke("DestroySelf", 3f);
    }
}
