using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI enemiesLeft;

    [SerializeField] private float timeForLevel;
    private int startingEnemyCount = 0;
    private int numEnemiesAlive = 0;

    private void Start()
    {
        enemiesLeft.text = numEnemiesAlive.ToString() + "/" + startingEnemyCount.ToString();
    }
    private void Update()
    {
        timeForLevel -= Time.deltaTime;
        timerText.text = timeForLevel.ToString();
        if (timeForLevel < 0)
            LoseRound();
        if (numEnemiesAlive == 0)
            WinRound();
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void LoseRound()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
