using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SimpleMenuController : MonoBehaviour
{
    [Header("Main Menu")]
    public int mainMenuBuildIndex = 0; // set in inspector

    // Call this from your "Main Menu" button
    public void GoToMainMenu()
    {
        // Ensure game isn't paused
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    // Call this from your "Exit Game" button
    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
