using UnityEngine;
using TMPro;

public class TextAutoHide : MonoBehaviour
{
    public float delay = 4f;  // time before hiding
    private TextMeshProUGUI tmp;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        CancelInvoke();
        Invoke(nameof(HideText), delay);
    }

    void HideText()
    {
        if (tmp != null)
            tmp.enabled = false;
        else
            gameObject.SetActive(false);
    }
}
