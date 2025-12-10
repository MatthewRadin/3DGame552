using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static bool isGamePaused = false;
    [SerializeField] private GameObject pauseMenuUI;
    private void Start()
    {
        isGamePaused = false;
        Time.timeScale = 1.0f;
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
        SceneManager.LoadScene(1);  // Load level 1 for now
    }
    public void HowToPlay()
    {
        Debug.Log("Hi");
    }
    public void About()
    {
        Debug.Log("About");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
