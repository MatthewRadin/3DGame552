using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static bool isGamePaused = false;
    [SerializeField] private GameObject pauseMenuUI;

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject levelsMenuUI;
    [SerializeField] private GameObject aboutUI;

    [Header("Level Interactable")]
    [SerializeField] private GameObject level01Button;
    [SerializeField] private GameObject level02Button;
    [SerializeField] private GameObject level03Button;
    private void Start()
    {
        isGamePaused = false;
        Time.timeScale = 1.0f;
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel");
        level02Button.GetComponent<Button>().interactable = currentLevel > 1;
        level03Button.GetComponent<Button>().interactable = currentLevel > 2;

    }
    void Update()
    {
        if (pauseMenuUI != null && Keyboard.current.pKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isGamePaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1.0f;
        isGamePaused = false;
    }
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0.0f;
        isGamePaused = true;
    }
    public void Menu()
    {
        Resume();
        SceneManager.LoadScene(0); //Main menu
    }
    public void PlayGame()
    {
        // Set active the level select screen
        mainMenuUI.SetActive(false);
        levelsMenuUI.SetActive(true);
    }
    public void HowToPlay()
    {
        Debug.Log("Hi");
    }
    public void About()
    {
        mainMenuUI.SetActive(false);
        aboutUI.SetActive(true);
    }
    public void BackToMain()
    {
        mainMenuUI.SetActive(true);
        levelsMenuUI.SetActive(false);
        aboutUI.SetActive(false);
    }
    public void Level1Load()
    {
        SceneManager.LoadScene(1);
    }
    public void Level2Load()
    {
        SceneManager.LoadScene(2);
    }
    public void Level3Load()
    {
        SceneManager.LoadScene(3);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
