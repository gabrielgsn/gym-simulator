using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    public float bobSpeed = 14f;
    public float bobAmount = 0.08f;
    public float pumpDuration = 0.6f;
    public float pumpAmount = 0.35f;

    SpriteRenderer spriteRenderer;
    Vector3 lastPosition;
    Vector3 baseScale;
    Color baseColor;
    float phase;
    float pumpTimer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseColor = spriteRenderer.color;
        lastPosition = transform.position;
    }

    public void Pump() => pumpTimer = pumpDuration;

    void LateUpdate()
    {
        Vector3 delta = transform.position - lastPosition;
        lastPosition = transform.position;

        if (Mathf.Abs(delta.x) > 0.001f)
            spriteRenderer.flipX = delta.x < 0f;

        if (delta.sqrMagnitude > 0.000001f)
            phase += Time.deltaTime * bobSpeed;
        else
            phase = 0f;

        float squash = Mathf.Abs(Mathf.Sin(phase)) * bobAmount;
        float pump = 1f;
        if (pumpTimer > 0f)
        {
            pumpTimer -= Time.deltaTime;
            float t = 1f - pumpTimer / pumpDuration;
            pump = 1f + Mathf.Sin(t * Mathf.PI) * pumpAmount;
            spriteRenderer.color = Color.Lerp(baseColor, new Color(1f, 0.85f, 0.4f), Mathf.Sin(t * Mathf.PI));
        }
        else
        {
            spriteRenderer.color = baseColor;
        }

        transform.localScale = new Vector3(baseScale.x * (1f + squash * 0.5f) * pump, baseScale.y * (1f - squash) * pump, baseScale.z);
    }
}
