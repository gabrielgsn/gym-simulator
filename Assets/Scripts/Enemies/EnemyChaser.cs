using UnityEngine;

public class EnemyChaser : Enemy
{
    public float chaseRadius = 4.5f;
    public float timePenalty = 5f;
    public float talkStun = 1.2f;

    Vector2 home;

    void Start() => home = rb.position;

    protected override Vector2 GetDirection()
    {
        bool chasing = cooldown <= 0f && Vector2.Distance(rb.position, player.position) <= chaseRadius;
        return DirectionTo(chasing ? (Vector2)player.position : home);
    }

    protected override void OnHitPlayer(PlayerMovement p, Vector2 away)
    {
        p.Knock(away * 3f, talkStun);
        GameManager.Instance.Damage(0f, timePenalty, $"\"Vai usar?\" Papo furado! -{timePenalty:0}s");
    }
}
