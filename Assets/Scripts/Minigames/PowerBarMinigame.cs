using System.Collections;
using UnityEngine;

public class PowerBarMinigame : MinigameBase
{
    public RectTransform meter;
    public RectTransform zone;
    public RectTransform pointer;

    public float startSpeed = 0.15f;
    public float acceleration = 1f;
    public float zoneCenterMin = 0.5f;
    public float zoneCenterMax = 0.8f;
    public float zoneStartHalf = 0.13f;
    public float zoneEndHalf = 0.08f;
    public float perfectRatio = 0.35f;
    public float standUpTime = 0.4f;

    protected override void OnSetup()
    {
        SetDepth(0f);
        zone.gameObject.SetActive(false);
    }

    protected override IEnumerator PlayRep()
    {
        float half = Mathf.Lerp(zoneStartHalf, zoneEndHalf, Progress);
        float center = Random.Range(zoneCenterMin, zoneCenterMax);
        float height = meter.rect.height;
        zone.anchoredPosition = new Vector2(zone.anchoredPosition.x, height / 2f - center * height);
        zone.sizeDelta = new Vector2(zone.sizeDelta.x, half * 2f * height);
        zone.gameObject.SetActive(true);
        ShowFeedback("SEGURE pra descer", Color.white);

        while (!hitAction.IsPressed())
        {
            if (CancelPressed())
                yield break;
            yield return null;
        }

        feedbackText.text = "";
        float depth = 0f;
        float speed = startSpeed;

        while (hitAction.IsPressed())
        {
            if (CancelPressed())
                yield break;

            speed += acceleration * Time.deltaTime;
            depth += speed * Time.deltaTime;
            if (depth >= 1f)
            {
                depth = 1f;
                break;
            }

            SetDepth(depth);
            yield return null;
        }

        SetDepth(depth);
        float error = Mathf.Abs(depth - center);
        if (error <= half * perfectRatio)
            Hit("PERFECT!", 300, PerfectColor);
        else if (error <= half)
            Hit("GOOD", 100, GoodColor);
        else if (depth < center)
            Miss("RASO!");
        else
            Miss("FUNDO DEMAIS!");

        yield return StandUp(depth);
    }

    IEnumerator StandUp(float from)
    {
        for (float t = 0f; t < standUpTime; t += Time.deltaTime)
        {
            SetDepth(Mathf.Lerp(from, 0f, t / standUpTime));
            yield return null;
        }
        SetDepth(0f);
    }

    void SetDepth(float depth)
    {
        float height = meter.rect.height;
        pointer.anchoredPosition = new Vector2(pointer.anchoredPosition.x, height / 2f - depth * height);
        SetBarbell(1f - depth);
    }
}
