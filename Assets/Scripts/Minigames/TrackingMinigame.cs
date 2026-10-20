using System.Collections;
using UnityEngine;

public class TrackingMinigame : MinigameBase
{
    public RectTransform track;
    public RectTransform band;
    public RectTransform cursor;
    public RectTransform progressFill;

    public float bandStartSize = 0.32f;
    public float bandEndSize = 0.24f;
    public float bandSpeed = 0.16f;
    public float riseAcceleration = 1.8f;
    public float fallAcceleration = 1.6f;
    public float maxSpeed = 0.8f;
    public float fillRate = 0.5f;
    public float drainRate = 0.2f;
    public float startProgress = 0.3f;

    protected override void OnSetup()
    {
        SetCursor(0f);
        SetFill(progressFill, 0f, false);
    }

    protected override IEnumerator PlayRep()
    {
        float size = Mathf.Lerp(bandStartSize, bandEndSize, Progress);
        float bandPos = 0.5f;
        float bandTarget = Random.Range(size / 2f, 1f - size / 2f);
        float cursorPos = 0.2f;
        float velocity = 0f;
        float progress = startProgress;
        bool leftBand = false;
        float speed = bandSpeed * (1f + 0.3f * Progress);

        ShowFeedback("Fique no verde!", Color.white);
        band.sizeDelta = new Vector2(band.sizeDelta.x, size * track.rect.height);

        while (true)
        {
            if (CancelPressed())
                yield break;

            if (Mathf.Abs(bandPos - bandTarget) < 0.01f || Random.value < 0.2f * Time.deltaTime)
                bandTarget = Random.Range(size / 2f, 1f - size / 2f);
            bandPos = Mathf.MoveTowards(bandPos, bandTarget, speed * Time.deltaTime);

            velocity += (hitAction.IsPressed() ? riseAcceleration : -fallAcceleration) * Time.deltaTime;
            velocity = Mathf.Clamp(velocity, -maxSpeed, maxSpeed);
            cursorPos += velocity * Time.deltaTime;
            if (cursorPos <= 0f || cursorPos >= 1f)
            {
                cursorPos = Mathf.Clamp01(cursorPos);
                velocity = 0f;
            }

            bool inside = Mathf.Abs(cursorPos - bandPos) <= size / 2f;
            if (inside)
            {
                progress += fillRate * Time.deltaTime;
            }
            else
            {
                progress -= drainRate * Time.deltaTime;
                leftBand = true;
            }

            SetBand(bandPos);
            SetCursor(cursorPos);
            SetFill(progressFill, progress, false);

            if (progress >= 1f)
            {
                if (!leftBand)
                    Hit("PERFECT!", 300, PerfectColor);
                else
                    Hit("GOOD", 100, GoodColor);
                break;
            }

            if (progress <= 0f)
            {
                Miss("PERDEU O CONTROLE!");
                break;
            }

            yield return null;
        }

        SetFill(progressFill, 0f, false);
    }

    void SetBand(float pos)
    {
        float h = track.rect.height;
        band.anchoredPosition = new Vector2(0f, pos * h - h / 2f);
    }

    void SetCursor(float pos)
    {
        float h = track.rect.height;
        cursor.anchoredPosition = new Vector2(0f, pos * h - h / 2f);
    }
}
