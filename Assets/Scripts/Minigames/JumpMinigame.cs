using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpMinigame : MinigameBase
{
    public RectTransform player;
    public RectTransform[] plates;

    public float groundY = -130f;
    public float playerX = -250f;
    public float spawnX = 440f;
    public float startSpeed = 420f;
    public float endSpeed = 640f;
    public float jumpVelocity = 820f;
    public float gravity = 2400f;
    public float releaseCut = 0.6f;
    public float doubleChance = 0.45f;
    public float plateGap = 58f;
    public float perfectDistance = 230f;

    float playerY;
    float velocityY;
    bool grounded = true;
    bool cut;

    protected override float RepPause => 0f;

    protected override void OnSetup()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerGrowth != null)
            player.GetComponent<Image>().sprite = GameManager.Instance.playerGrowth.CurrentSprite;
        playerY = 0f;
        velocityY = 0f;
        grounded = true;
        PlacePlayer();
        HidePlates();
    }

    protected override void OnCleanup() => HidePlates();

    protected override IEnumerator PlayRep()
    {
        int count = Random.value < doubleChance * Progress ? 2 : 1;
        float speed = Mathf.Lerp(startSpeed, endSpeed, Progress);
        float size = plates[0].rect.width;
        float halfSpan = (count - 1) * plateGap / 2f + size / 2f;
        float playerHalfWidth = player.rect.width * 0.35f;
        float x = spawnX;
        float angle = 0f;
        float jumpDistance = -1f;

        for (int i = 0; i < plates.Length; i++)
            plates[i].gameObject.SetActive(i < count);

        if (reps == 0 && misses == 0)
            ShowFeedback("PULE!", Color.white);

        while (true)
        {
            if (CancelPressed())
                yield break;

            float center = x + (count - 1) * plateGap / 2f;
            if (UpdatePlayer() && jumpDistance < 0f)
                jumpDistance = center - playerX;

            x -= speed * Time.deltaTime;
            angle += speed * Time.deltaTime / (size / 2f) * Mathf.Rad2Deg;
            for (int i = 0; i < count; i++)
            {
                plates[i].anchoredPosition = new Vector2(x + i * plateGap, groundY + size / 2f);
                plates[i].localEulerAngles = new Vector3(0f, 0f, angle);
            }

            center = x + (count - 1) * plateGap / 2f;
            bool overlapX = Mathf.Abs(center - playerX) < halfSpan + playerHalfWidth;
            if (overlapX && playerY < size * 0.8f)
            {
                Miss("TROPEÇOU!");
                break;
            }

            if (center + halfSpan < playerX - playerHalfWidth)
            {
                if (jumpDistance > 0f && jumpDistance <= perfectDistance + halfSpan)
                    Hit("PERFECT!", 300, PerfectColor);
                else
                    Hit("PULOU!", 100, GoodColor);
                break;
            }

            yield return null;
        }

        HidePlates();
    }

    bool UpdatePlayer()
    {
        bool jumped = false;
        if (grounded && hitAction.WasPressedThisFrame())
        {
            velocityY = jumpVelocity;
            grounded = false;
            cut = false;
            jumped = true;
        }

        if (!grounded)
        {
            if (!cut && velocityY > 0f && !hitAction.IsPressed())
            {
                velocityY *= releaseCut;
                cut = true;
            }

            velocityY -= gravity * Time.deltaTime;
            playerY += velocityY * Time.deltaTime;
            if (playerY <= 0f)
            {
                playerY = 0f;
                velocityY = 0f;
                grounded = true;
            }
        }

        PlacePlayer();
        return jumped;
    }

    void PlacePlayer()
    {
        player.anchoredPosition = new Vector2(playerX, groundY + player.rect.height / 2f + playerY);
    }

    void HidePlates()
    {
        foreach (var p in plates)
            p.gameObject.SetActive(false);
    }
}
