using UnityEngine;

public class EnemyPatrol : Enemy
{
    public Vector2[] waypoints;
    public float staminaDamage = 15f;
    public float knockbackForce = 9f;

    int index;

    protected override Vector2 GetDirection()
    {
        if (waypoints.Length == 0)
            return Vector2.zero;

        if (Vector2.Distance(rb.position, waypoints[index]) < 0.1f)
            index = (index + 1) % waypoints.Length;

        return DirectionTo(waypoints[index]);
    }

    protected override void OnHitPlayer(PlayerMovement p, Vector2 away)
    {
        p.Knock(away * knockbackForce, 0.35f);
        GameManager.Instance.Damage(staminaDamage, 0f, $"Esbarrou no marombeiro! -{staminaDamage:0} energia");
    }
}
