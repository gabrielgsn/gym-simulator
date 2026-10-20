using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState { Playing, Minigame, Paused, Ended }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public float timeLimit = 180f;
    public float maxStamina = 100f;
    public float staminaRegen = 1.5f;
    public float setCost = 20f;
    public float failCost = 10f;
    public float shakeStamina = 35f;
    public int shakeScore = 50;
    public int timeBonusPerSecond = 10;

    public List<GymMachine> machines = new();
    public Hud hud;
    public PlayerGrowth playerGrowth;
    public GameObject pausePanel;
    public GameObject pauseFirstButton;
    public GameObject endPanel;
    public GameObject endFirstButton;
    public Text endTitle;
    public Text endStats;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip deniedClip;
    public AudioClip shakeClip;

    public GameState State { get; private set; }
    public float TimeLeft { get; private set; }
    public float Stamina { get; private set; }
    public int Score { get; private set; }
    public float Elapsed => timeLimit - TimeLeft;
    public int CompletedCount => machines.Count(m => m.Completed);
    public bool CanInteract => State == GameState.Playing && Time.frameCount > inputBlockedUntil;

    MinigameBase activeMinigame;
    InputAction pauseAction;
    int inputBlockedUntil;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
        TimeLeft = timeLimit;
        Stamina = maxStamina;
        State = GameState.Playing;

        pauseAction = new InputAction("Pause", InputActionType.Button);
        pauseAction.AddBinding("<Keyboard>/escape");
        pauseAction.AddBinding("<Keyboard>/p");
        pauseAction.AddBinding("<Gamepad>/start");
    }

    void OnEnable() => pauseAction.Enable();
    void OnDisable() => pauseAction.Disable();

    void Start()
    {
        pausePanel.SetActive(false);
        endPanel.SetActive(false);
        hud.RefreshChecklist(machines);
    }

    void Update()
    {
        if (pauseAction.WasPressedThisFrame() && Time.frameCount > inputBlockedUntil)
        {
            if (State == GameState.Playing)
                Pause();
            else if (State == GameState.Paused)
                Resume();
        }

        if (State != GameState.Playing && State != GameState.Minigame)
            return;

        TimeLeft -= Time.deltaTime;
        if (State == GameState.Playing)
            Stamina = Mathf.Min(maxStamina, Stamina + staminaRegen * Time.deltaTime);

        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            EndGame(false);
        }
    }

    public bool TryStartSet(MinigameBase minigame)
    {
        if (!CanInteract)
            return false;

        if (Stamina < setCost)
        {
            sfxSource.PlayOneShot(deniedClip);
            hud.Toast("Sem energia! Pegue um shake", Hud.BadColor);
            return false;
        }

        State = GameState.Minigame;
        activeMinigame = minigame;
        return true;
    }

    public void EndSet(GymMachine machine, bool success, int score)
    {
        activeMinigame = null;
        if (State == GameState.Ended)
            return;

        State = GameState.Playing;
        inputBlockedUntil = Time.frameCount + 1;
        Stamina = Mathf.Max(0f, Stamina - (success ? setCost : failCost));
        Score += score;

        if (!success)
        {
            hud.Toast($"{machine.exerciseName}: série falhou", Hud.BadColor);
            return;
        }

        bool grew = playerGrowth != null && playerGrowth.SetCompleted(CompletedCount);
        hud.Toast(grew ? $"{machine.exerciseName} concluído! Ficou mais bombado!" : $"{machine.exerciseName} concluído! +{score}", Hud.GoodColor);
        hud.RefreshChecklist(machines);
        if (CompletedCount == machines.Count)
            EndGame(true);
    }

    public void CollectShake()
    {
        Stamina = Mathf.Min(maxStamina, Stamina + shakeStamina);
        Score += shakeScore;
        sfxSource.PlayOneShot(shakeClip);
        hud.Toast($"Shake! +{shakeStamina:0} energia", Hud.GoodColor);
    }

    public void Damage(float stamina, float seconds, string message)
    {
        Stamina = Mathf.Max(0f, Stamina - stamina);
        TimeLeft = Mathf.Max(0f, TimeLeft - seconds);
        hud.Toast(message, Hud.BadColor);
    }

    void Pause()
    {
        State = GameState.Paused;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
    }

    public void Resume()
    {
        if (State != GameState.Paused)
            return;

        State = GameState.Playing;
        Time.timeScale = 1f;
        inputBlockedUntil = Time.frameCount + 1;
        pausePanel.SetActive(false);
    }

    void EndGame(bool won)
    {
        State = GameState.Ended;
        if (activeMinigame != null)
            activeMinigame.Abort();

        Time.timeScale = 0f;
        musicSource.Stop();
        sfxSource.PlayOneShot(won ? winClip : loseClip);

        int bonus = won ? Mathf.CeilToInt(TimeLeft) * timeBonusPerSecond : 0;
        Score += bonus;

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (Score > bestScore)
        {
            bestScore = Score;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }
        if (won)
        {
            float bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
            if (Elapsed < bestTime)
                PlayerPrefs.SetFloat("BestTime", Elapsed);
        }
        PlayerPrefs.Save();

        endTitle.text = won ? "TREINO COMPLETO!" : "A ACADEMIA FECHOU!";
        endTitle.color = won ? Hud.GoodColor : Hud.BadColor;
        endStats.text =
            $"Tempo final: {FormatTime(Elapsed)}\n" +
            $"Exercícios: {CompletedCount}/{machines.Count}\n" +
            $"Bônus de tempo: +{bonus}\n" +
            $"Score: {Score}\n" +
            $"Recorde: {bestScore}";

        pausePanel.SetActive(false);
        endPanel.SetActive(true);
        inputBlockedUntil = Time.frameCount + 1;
        EventSystem.current.SetSelectedGameObject(endFirstButton);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    public static string FormatTime(float seconds)
    {
        int s = Mathf.CeilToInt(seconds);
        return $"{s / 60:00}:{s % 60:00}";
    }
}
