using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Tooltip("Build index of the first playable level (not main menu).")]
    public int firstLevelBuildIndex = 1;

    public void OnContinuePressed()
    {
        int saved = ProgressManager.GetSavedSceneIndex();
        if (saved >= 0)
        {
            SceneManager.LoadScene(saved);
        }
        else
        {
            SceneManager.LoadScene(firstLevelBuildIndex);
        }
    }

    public void OnNewGamePressed()
    {
        ProgressManager.ClearProgress();
        SceneManager.LoadScene(firstLevelBuildIndex);
    }

    public void OnQuitPressed()
    {
        Application.Quit();
    }
}
