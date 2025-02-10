using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public RectTransform sideMenuArea; // Assign this in the Inspector
    public Canvas mainCanvas;         // Assign your Canvas in the Inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional if you want to persist it across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
