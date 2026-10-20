using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public float speed = 2.5f;
    public float hitCooldown = 2f;
    public Transform player;
    public AudioClip hitClip;

    protected Rigidbody2D rb;
    protected AudioSource audioSource;
    protected float cooldown;

    protected bool Active => GameManager.Instance == null || GameManager.Instance.State == GameState.Playing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = Vector2.zero;
        if (!Active)
            return;

        cooldown -= Time.fixedDeltaTime;
        Vector2 dir = GetDirection();
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }

    protected Vector2 DirectionTo(Vector2 target)
    {
        Vector2 diff = target - rb.position;
        return diff.sqrMagnitude < 0.01f ? Vector2.zero : diff.normalized;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!Active || cooldown > 0f)
            return;

        var p = collision.collider.GetComponent<PlayerMovement>();
        if (p == null)
            return;

        cooldown = hitCooldown;
        audioSource.PlayOneShot(hitClip);
        OnHitPlayer(p, ((Vector2)p.transform.position - rb.position).normalized);
    }

    protected abstract Vector2 GetDirection();
    protected abstract void OnHitPlayer(PlayerMovement p, Vector2 away);
}
