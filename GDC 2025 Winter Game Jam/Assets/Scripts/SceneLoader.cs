using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoadButton : MonoBehaviour
{
    public Button button;

    public string sceneName;

    void Awake()
    {
        if (button != null)
            button.onClick.AddListener(LoadScene);
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}