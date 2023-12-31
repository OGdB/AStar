using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Standard pause screen script that should have most functionalities you need for pause screens.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    #region Properties
    [SerializeField, Range(0.25f, 2f), Tooltip("The speed at which the menu appears/disappears")]
    private float appearSpeed = 0.5f;

    [SerializeField, Space(10)]
    private KeyCode[] pauseButtons = new KeyCode[] { KeyCode.Escape };

    [SerializeField, Space(10), Tooltip("Events invoked when the application is paused.")]
    private UnityEvent onPauseEvents;
    [SerializeField, Tooltip("Events invoked when the application is resumed.")]
    private UnityEvent onResumeEvents;

    public static bool isPaused = false;
    #endregion

    // Can be changed to be invoked by the new input system.
    private void Update()
    {
        bool pausePressed = false;
        
        // Iterate over all set pause buttons to see if it is pressed.
        // New input system is probably more efficient.
        foreach (var pauseButton in pauseButtons)
        {
            if (Input.GetKeyDown(pauseButton))
                pausePressed = true;
        }

        if (pausePressed)
            SwitchPauseState();
    }

    /// <summary>
    /// Either pauses or resumes the application.
    /// </summary>
    public void SwitchPauseState()
    {
        StopAllCoroutines();
        // Not bothering to store in variable due to infrequent need of variable.
        CanvasGroup canvas = GetComponentInChildren<CanvasGroup>();
        StartCoroutine(SwitchPauseScreenCR(canvas, appearSpeed));
    }

    public void RestartLevel()
    {
        isPaused = false;
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private IEnumerator SwitchPauseScreenCR(CanvasGroup canvas, float speed)
    {
        // Change pause state on invoke.
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f; // Resume time if unpausing, vice versa.

        WaitForEndOfFrame frame = new();

        // Set target alpha
        float startAlpha = canvas.alpha;
        float targetAlpha = 1f;
        // Unscaled time as otherwise the coroutine will cease to function due to timescale change.
        float startTime = Time.unscaledTime;

        // If it is paused now, set resume targets, vice versa.
        if (isPaused)
        {
            canvas.interactable = true;
            canvas.blocksRaycasts = true;
            onPauseEvents?.Invoke();
        }
        else
        {
            targetAlpha = 0f;
            canvas.interactable = false;
            canvas.blocksRaycasts = false;
            onResumeEvents?.Invoke();
        }

        while (canvas.alpha != targetAlpha)
        {
            float timeSinceStarted = Time.unscaledTime - startTime;
            float progress = timeSinceStarted / speed;

            float alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

            canvas.alpha = alpha;

            yield return frame;
        }
    }
}
