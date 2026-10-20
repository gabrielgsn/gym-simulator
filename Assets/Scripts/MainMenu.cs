using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject howToPanel;
    public GameObject firstButton;
    public GameObject howToBackButton;
    public Text recordText;

    void Start()
    {
        Time.timeScale = 1f;
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        float bestTime = PlayerPrefs.GetFloat("BestTime", -1f);
        recordText.text = bestScore > 0
            ? $"Recorde: {bestScore}" + (bestTime > 0f ? $"   |   Melhor tempo: {GameManager.FormatTime(bestTime)}" : "")
            : "";
        HideHowTo();
    }

    public void Play() => SceneManager.LoadScene("Game");

    public void ShowHowTo()
    {
        mainPanel.SetActive(false);
        howToPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(howToBackButton);
    }

    public void HideHowTo()
    {
        howToPanel.SetActive(false);
        mainPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstButton);
    }
}
