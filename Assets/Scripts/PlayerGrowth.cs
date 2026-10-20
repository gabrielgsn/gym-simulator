using UnityEngine;

public class PlayerGrowth : MonoBehaviour
{
    public Sprite[] stages;
    public SpriteRenderer spriteRenderer;
    public CharacterVisual visual;
    public int exercisesPerStage = 2;

    public int Stage { get; private set; }
    public Sprite CurrentSprite => stages[Stage];

    void Start() => spriteRenderer.sprite = CurrentSprite;

    public bool SetCompleted(int completed)
    {
        int stage = Mathf.Clamp(Mathf.CeilToInt(completed / (float)exercisesPerStage), 0, stages.Length - 1);
        if (stage <= Stage)
            return false;

        Stage = stage;
        spriteRenderer.sprite = CurrentSprite;
        visual.Pump();
        return true;
    }
}
