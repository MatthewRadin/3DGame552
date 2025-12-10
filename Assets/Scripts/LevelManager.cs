using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private int levelNumber;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI enemiesLeft;

    [SerializeField] private float timeForLevel;
    [SerializeField] private MenuManager menuManager;
    private int startingEnemyCount = 0;
    private int numEnemiesAlive = 0;

    private void Start()
    {
        enemiesLeft.text = numEnemiesAlive.ToString() + "/" + startingEnemyCount.ToString();
    }
    private void Update()
    {
        timeForLevel -= Time.deltaTime;
        UpdateTimerUI();
        if (timeForLevel < 0)
            LoseRound();
        if (numEnemiesAlive == 0)
            WinRound();
    }
    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        timerText.text = FormatTime(timeForLevel);
    }
    private string FormatTime(float time)
    {
        time = Mathf.Max(0f, time);

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time);
        int milliseconds = Mathf.FloorToInt((time % 1f) * 1000f);

        return string.Format("{0:00}:{1:000}", seconds, milliseconds);
    }

    public void IncrementEnemy()
    {
        numEnemiesAlive++;
        startingEnemyCount++;
    }
    public void EnemyKilled()
    {
        timeForLevel += 1;
        numEnemiesAlive--;
        enemiesLeft.text = numEnemiesAlive.ToString() + "/" + startingEnemyCount.ToString();
    }
    private void WinRound()
    {
        if (levelNumber + 1 > PlayerPrefs.GetInt("CurrentLevel"))
            PlayerPrefs.SetInt("CurrentLevel", levelNumber + 1);

        menuManager.ShowWinMenu();
    }
    private void LoseRound()
    {
        menuManager.ShowLoseMenu();
    }
}
