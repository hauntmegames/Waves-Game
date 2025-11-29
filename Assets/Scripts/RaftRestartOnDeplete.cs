// RaftRestartOnDeplete.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaftRestartOnDeplete : MonoBehaviour
{
    [Tooltip("If > 0, this default delay will be used when RestartAfterDelay is called without parameter.")]
    public float defaultDelay = 1.0f;

    // Public so you can wire this method directly into RaftCondition.onDepleted
    public void RestartNow()
    {
        // immediate reload of active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Public so you can hook this method as OnDepleted -> RestartAfterDelay (Single) and pass a float if you like
    public void RestartAfterDelay(float seconds)
    {
        StartCoroutine(RestartCoroutine(seconds));
    }

    // Overload: call without parameter (useful if you set the UnityEvent to call RestartAfterDelay with no argument)
    public void RestartAfterDefaultDelay()
    {
        StartCoroutine(RestartCoroutine(defaultDelay));
    }

    IEnumerator RestartCoroutine(float delaySeconds)
    {
        if (delaySeconds > 0f)
        {
            // use unscaled time so Time.timeScale changes (e.g. pausing) won't break the delay
            float t = 0f;
            while (t < delaySeconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // Optional: play a fade here (call your fade UI)
        // Example: FadeUI.Instance?.FadeOut(0.5f); yield return new WaitForSecondsRealtime(0.5f);

        // reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
