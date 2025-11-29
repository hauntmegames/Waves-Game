using UnityEngine;
using TMPro;
using PixelCrushers.DialogueSystem;

public class ShellCounterUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI label;          // drag your TMP text here
    public string format = "{0}";          // e.g., "Shells: {0}" or just "{0}"

    [Header("Dialogue System")]
    public string variableName = "ShellsCollected";

    void Reset()
    {
        label = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (!label) return;
        int n = DialogueLua.GetVariable(variableName).asInt;
        label.text = string.Format(format, n);
    }
}
