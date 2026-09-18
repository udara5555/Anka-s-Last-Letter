using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    private static readonly Stack<string> SceneHistory = new Stack<string>();

    public void LoadScene(string sceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != sceneName)
        {
            SceneHistory.Push(currentScene);
        }

        SceneManager.LoadScene(sceneName);
    }

    public void LoadPreviousScene()
    {
        if (SceneHistory.Count == 0)
        {
            return;
        }

        SceneManager.LoadScene(SceneHistory.Pop());
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
