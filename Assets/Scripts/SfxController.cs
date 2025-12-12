using UnityEngine;

public class SfxController : MonoBehaviour
{
    [Header("Player Sounds")]
    [SerializeField] private AudioClip playerDeathSound;
    [SerializeField] private AudioClip playerJumpSound;
    [SerializeField] private AudioClip playerShootSound;
    
    [Header("Enemy Sounds")]
    [SerializeField] private AudioClip enemyDeathSound;
    
    [Header("Hit Sounds")]
    [SerializeField] private AudioClip paintballHitmarkerSound;

    private AudioSource audioSource;
    private static SfxController instance;

    private void Awake()
    {
        // Singleton pattern - allow multiple instances per scene but keep one global reference
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure for SFX (not music)
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    // Public static methods for easy access from any script
    public static void PlayPlayerDeath()
    {
        if (instance != null && instance.playerDeathSound != null)
        {
            instance.audioSource.PlayOneShot(instance.playerDeathSound);
        }
    }

    public static void PlayPlayerJump()
    {
        if (instance != null && instance.playerJumpSound != null)
        {
            instance.audioSource.PlayOneShot(instance.playerJumpSound);
        }
    }

    public static void PlayPlayerShoot()
    {
        if (instance != null && instance.playerShootSound != null)
        {
            instance.audioSource.PlayOneShot(instance.playerShootSound);
        }
    }

    public static void PlayEnemyDeath()
    {
        if (instance != null && instance.enemyDeathSound != null)
        {
            instance.audioSource.PlayOneShot(instance.enemyDeathSound);
        }
    }

    public static void PlayPaintballHitmarker()
    {
        if (instance != null && instance.paintballHitmarkerSound != null)
        {
            instance.audioSource.PlayOneShot(instance.paintballHitmarkerSound);
        }
    }
}
