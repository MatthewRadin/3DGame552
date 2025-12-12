using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class MusicClass : MonoBehaviour
{
    [SerializeField] private AudioClip primaryClip;
    [SerializeField] private AudioClip secondaryClip;
    [SerializeField] private AudioClip tertiaryClip;

    [Header("Menu/Tutorial Scenes (Primary Music)")]
    [SerializeField] private string[] menuSceneNames = { "MainMenu", "Sandbox" };

    private AudioSource audioSource;
    private Coroutine switchCoroutine;
    private static MusicClass instance;
    private bool isPlayingSecondary = false;
    private bool isPlayingTertiary = false;
    private bool waitingToSwitch = false;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log($"MusicController persisting: {gameObject.name}");

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Determine initial scene and play appropriate music
        Scene currentScene = SceneManager.GetActiveScene();
        if (IsMenuScene(currentScene.name))
        {
            PlayPrimaryMusic();
        }
        else
        {
            // Starting in a level - play tertiary immediately
            PlayTertiaryMusic();
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() 
    { 
        Debug.Log($"MusicController being destroyed: {gameObject.name}");
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if we're in a menu/tutorial scene
        if (IsMenuScene(scene.name))
        {
            // Don't reset music when returning to menu - keep playing current track
            waitingToSwitch = false;
            if (switchCoroutine != null)
            {
                StopCoroutine(switchCoroutine);
                switchCoroutine = null;
            }
        }
        else
        {
            // In a level - start the transition sequence ONLY if still on primary music
            if (!isPlayingSecondary && !isPlayingTertiary && !waitingToSwitch)
            {
                WaitAndSwitchToSecondary();
            }
            // If already on secondary or tertiary, just let it continue playing
        }
    }

    private bool IsMenuScene(string sceneName)
    {
        foreach (string menuScene in menuSceneNames)
        {
            if (sceneName == menuScene)
                return true;
        }
        return false;
    }

    private void PlayPrimaryMusic()
    {
        if (primaryClip == null) return;
        
        audioSource.clip = primaryClip;
        audioSource.loop = true;
        audioSource.volume = 1f;
        audioSource.Play();
        isPlayingSecondary = false;
        isPlayingTertiary = false;
    }

    private void PlaySecondaryMusic()
    {
        if (secondaryClip == null) return;
        
        // Only restart if not already playing secondary
        if (audioSource.clip != secondaryClip || !audioSource.isPlaying)
        {
            audioSource.clip = secondaryClip;
            audioSource.loop = false; // Don't loop, will switch to tertiary after
            audioSource.volume = 1f;
            audioSource.Play();
        }
        
        isPlayingSecondary = true;
        isPlayingTertiary = false;
        waitingToSwitch = false;
        
        // Queue switch to tertiary after secondary finishes (only if not already queued)
        if (switchCoroutine == null)
        {
            switchCoroutine = StartCoroutine(WaitForSecondaryToEnd());
        }
    }

    private void PlayTertiaryMusic()
    {
        if (tertiaryClip == null) return;
        
        audioSource.clip = tertiaryClip;
        audioSource.loop = true; // Loop forever
        audioSource.volume = 1f;
        audioSource.Play();
        isPlayingSecondary = false;
        isPlayingTertiary = true;
        waitingToSwitch = false;
    }

    private void WaitAndSwitchToSecondary()
    {
        if (switchCoroutine != null) StopCoroutine(switchCoroutine);
        switchCoroutine = StartCoroutine(WaitForSongEnd());
    }

    private System.Collections.IEnumerator WaitForSongEnd()
    {
        waitingToSwitch = true;
        
        if (primaryClip == null || audioSource == null)
        {
            waitingToSwitch = false;
            yield break;
        }

        // Calculate time remaining in primary song
        float timeRemaining = primaryClip.length - audioSource.time;
        
        // Wait for song to finish
        yield return new WaitForSeconds(timeRemaining);
        
        // Switch to secondary
        PlaySecondaryMusic();
        
        switchCoroutine = null;
    }

    private System.Collections.IEnumerator WaitForSecondaryToEnd()
    {
        if (secondaryClip == null || audioSource == null)
        {
            yield break;
        }

        // Calculate time remaining in secondary song based on current position
        float timeRemaining = secondaryClip.length - audioSource.time;
        
        // Wait for secondary song to finish
        yield return new WaitForSeconds(timeRemaining);
        
        // Switch to tertiary (infinite loop)
        PlayTertiaryMusic();
        
        switchCoroutine = null;
    }

    public void PlayMusic()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }
    public void StopMusic()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.Stop();
    }
}
