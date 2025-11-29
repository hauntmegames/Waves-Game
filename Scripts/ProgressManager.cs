using UnityEngine;
using UnityEngine.SceneManagement;

public static class ProgressManager
{
    private const string LastSceneKey = "Raft_LastSceneIndex";

    // Save the build index of the scene the player should resume at.
    public static void SaveLastSceneIndex(int buildIndex)
    {
        PlayerPrefs.SetInt(LastSceneKey, buildIndex);
        PlayerPrefs.Save();
        Debug.Log($"ProgressManager: Saved last scene index {buildIndex}");
    }

    // Returns true if a saved scene exists.
    public static bool HasSavedScene()
    {
        return PlayerPrefs.HasKey(LastSceneKey);
    }

    // Get saved index (safe: clamps to available scenes)
    public static int GetSavedSceneIndex()
    {
        if (!HasSavedScene()) return -1;
        int idx = PlayerPrefs.GetInt(LastSceneKey, -1);
        // Clamp to range [0, SceneCount-1]
        int maxIndex = SceneManager.sceneCountInBuildSettings - 1;
        if (idx < 0 || idx > maxIndex) return -1;
        return idx;
    }

    // Clear progress (New Game)
    public static void ClearProgress()
    {
        PlayerPrefs.DeleteKey(LastSceneKey);
        PlayerPrefs.Save();
        Debug.Log("ProgressManager: Cleared saved progress.");
    }
}
