using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;


public class MusicClass : MonoBehaviour
{
    [SerializeField] private AudioClip primaryClip;
    [SerializeField] private AudioClip secondaryClip;
    [SerializeField, HideInInspector] private float primaryLoopTriggerTime = -1f;
    [SerializeField, HideInInspector] private float primaryLoopStartTime = 0f;
    [SerializeField] private bool secondaryLoops = true;
    [SerializeField] private SceneAsset triggerSceneAsset = null;
    [SerializeField, HideInInspector] private string triggerSceneName = "";

    private AudioSource audioSource;
    private Coroutine switchCoroutine;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        if (primaryClip != null)
        {
            audioSource.clip = primaryClip;
            audioSource.time = 0f;
            audioSource.Play();
            audioSource.loop = primaryLoopStartTime <= 0f;
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

#if UNITY_EDITOR
private void OnValidate()
{
    if (triggerSceneAsset != null)
    {
        triggerSceneName = triggerSceneAsset.name;
    }
}
#endif

private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (triggerSceneAsset == null) return;
    if (scene.name != triggerSceneName) return;

    if (audioSource == null || audioSource.clip != primaryClip || !audioSource.isPlaying) return;
    float trigger = primaryLoopTriggerTime > 0f ? primaryLoopTriggerTime : primaryClip.length;
    float current = Mathf.Clamp(audioSource.time, 0f, primaryClip.length);
    float delay = trigger >= current ? trigger - current : (primaryClip.length - current) + trigger;
    if (switchCoroutine != null) StopCoroutine(switchCoroutine);
    switchCoroutine = StartCoroutine(SwitchAfterDelay(delay));
}

    private System.Collections.IEnumerator SwitchAfterDelay(float d)
    {
        yield return new WaitForSeconds(d);
        if (audioSource == null || audioSource.clip != primaryClip || !audioSource.isPlaying) { switchCoroutine = null; yield break; }
        if (secondaryClip != null)
        {
            audioSource.clip = secondaryClip;
            audioSource.time = 0f;
            audioSource.Play();
            audioSource.loop = secondaryLoops;
        }
        else
        {
            if (primaryLoopStartTime <= 0f)
                audioSource.loop = true;
            else
                audioSource.time = Mathf.Clamp(primaryLoopStartTime, 0f, primaryClip.length);
        }
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
