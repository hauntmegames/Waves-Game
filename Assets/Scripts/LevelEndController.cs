using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndController : MonoBehaviour
{
    // Called by the level's Continue button
    public void OnContinueButton()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        int maxIndex = SceneManager.sceneCountInBuildSettings - 1;

        if (nextIndex <= maxIndex)
        {
            ProgressManager.SaveLastSceneIndex(nextIndex);
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            // If last level, go back to main menu (index 0)
            ProgressManager.SaveLastSceneIndex(0);
            SceneManager.LoadScene(0);
        }
    }

    // Call this when the raft breaks to restart the current level
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
