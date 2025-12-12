using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider))]
public class SceneLoaderTrigger : MonoBehaviour
{
    #if UNITY_EDITOR
        public SceneAsset sceneAsset;
    #endif
    [HideInInspector] public string sceneName;
    [HideInInspector] public string playerTag = "Player";
    [HideInInspector] public float delay = 0f;
    [HideInInspector] public bool useAsync = true;
    [HideInInspector] bool triggered = false;

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"'{name}': Collider was not set as Trigger. Setting isTrigger = true.");
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        TryLoad(other.gameObject);
    }

    void OnTriggerEnter2D(UnityEngine.Collider2D other)
    {
        TryLoad(other.gameObject);
    }

    void TryLoad(GameObject other)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (triggered) return;

        if (!string.IsNullOrEmpty(playerTag) && !other.CompareTag(playerTag))
            return;

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"'{name}': Scene name is empty. Set sceneName on the {nameof(SceneLoaderTrigger)} component.");
            return;
        }

        triggered = true;

        if (delay > 0f)
        {
            StartCoroutine(LoadAfterDelay());
        }
        else
        {
            LoadScene();
        }
    }

    System.Collections.IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        LoadScene();
    }

    void LoadScene()
    {
        if (useAsync)
            SceneManager.LoadSceneAsync(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

#if UNITY_EDITOR
    // Keep sceneName in sync with the SceneAsset you drop into the inspector.
    void OnValidate()
    {
        // If sceneAsset is set, extract its filename and copy to sceneName
        if (sceneAsset != null)
        {
            var path = AssetDatabase.GetAssetPath(sceneAsset);
            if (!string.IsNullOrEmpty(path))
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (sceneName != name)
                {
                    sceneName = name;
                    // Mark the object dirty so the change is saved
                    EditorUtility.SetDirty(this);
                }

                // Check whether scene is in Build Settings and warn if not
                bool inBuild = false;
                foreach (var s in EditorBuildSettings.scenes)
                {
                    if (s.path == path)
                    {
                        inBuild = true;
                        break;
                    }
                }
                if (!inBuild)
                {
                    Debug.LogWarning($"Scene '{name}' (asset at '{path}') is not in Build Settings. Add it via File → Build Settings → Add Open Scenes or drag it into the Scenes In Build list.");
                }
            }
        }
    }
#endif
}
