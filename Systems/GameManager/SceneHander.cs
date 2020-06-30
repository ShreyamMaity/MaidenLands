using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;


public class SceneHander : MonoBehaviour
{
    public static SceneHander Instance;

    // scene transition
    public float transitionSmoothTime = 0.1f;
    public string targetScene;
    public string currentSceneName;
    public CanvasGroup canvasGroup;

    List<AsyncOperation> _loadOperations;

    void Awake()
    {
        if(Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        _loadOperations = new List<AsyncOperation>();
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            StopCoroutine("LoadScene");
            StartCoroutine("LoadScene");
        }
    }

    IEnumerator SceneTransitionWithAlpha(float targetAlpha)
    {
        float diff = Mathf.Abs(canvasGroup.alpha - targetAlpha);
        float transitionRate = 0;

        while(diff > 0.025f)
        {
            canvasGroup.alpha = Mathf.SmoothDamp(canvasGroup.alpha, targetAlpha, ref transitionRate, transitionSmoothTime);
            diff = Mathf.Abs(canvasGroup.alpha - targetAlpha);
            yield return new WaitForSeconds(Time.deltaTime);
        }

        canvasGroup.alpha = targetAlpha;
        if(targetAlpha == 1)
        {
            // StartCoroutine(LoadScene());
        }
    }

    // load level using co-routines
    IEnumerator LoadScene()
    {
        LoadLevel(targetScene);
        string activeScene = SceneManager.GetActiveScene().name;

        while(activeScene != targetScene)
        {
            activeScene = SceneManager.GetActiveScene().name;
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    // load scene using async operations
    public void LoadLevel(string levelName)
    {
        AsyncOperation ao = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        if (ao == null)
        {
            Debug.LogError("[GameManager] Unable to load level " + levelName);
            return;
        }

        ao.completed += OnLoadOperationComplete;
        _loadOperations.Add(ao);
        currentSceneName = levelName;
    }

    void OnLoadOperationComplete(AsyncOperation ao)
    {
        if (_loadOperations.Contains(ao))
        {
            _loadOperations.Remove(ao);
            if (_loadOperations.Count == 0)
            {
                StopCoroutine("SceneTransitionWithAlpha");
                StartCoroutine("SceneTransitionWithAlpha", 0);
                // UpdateState(GameState.RUNNING);
            }
        }
    }

    void OnUnloadOperationComplete(AsyncOperation ao)
    {
        // Clean up level is necessary, go back to main menu
    }
}
