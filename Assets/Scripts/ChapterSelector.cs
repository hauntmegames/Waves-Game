using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterSelector : MonoBehaviour
{
    // Call this from a button to load a specific scene by its build index
    public void LoadLevelByIndex(int buildIndex)
    {
        int maxIndex = SceneManager.sceneCountInBuildSettings - 1;

        if (buildIndex >= 0 && buildIndex <= maxIndex)
        {
            // Optional: save progress so we know where the player went
            ProgressManager.SaveLastSceneIndex(buildIndex);

            // Load the selected scene
            SceneManager.LoadScene(buildIndex);
        }
        else
        {
            Debug.LogWarning("ChapterSelector: Build index " + buildIndex + " is out of range!");
        }
    }
}
