using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSaveDataUpdater : MonoBehaviour
{
    private void Start()
    {
        string backgroundImageName = "";

        // Automatically get the Image component from the GameObject this script is attached to
        Image panelImage = GetComponent<Image>();
        if (panelImage != null && panelImage.sprite != null)
        {
            backgroundImageName = panelImage.sprite.name;
        }
        else
        {
            Debug.LogWarning("SceneSaveDataUpdater couldn't find an Image component with a Sprite on this GameObject!");
        }

        // Update the global StoryManager
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.currentSceneName = SceneManager.GetActiveScene().name;
            StoryManager.Instance.currentBackgroundImageName = backgroundImageName;
            
            Debug.Log($"Updated Save Data: Scene = {StoryManager.Instance.currentSceneName}, Background = {backgroundImageName}");
        }
        else
        {
            Debug.LogWarning("StoryManager was not found! Did you start the game from the 1st scene?");
        }
    }
}
