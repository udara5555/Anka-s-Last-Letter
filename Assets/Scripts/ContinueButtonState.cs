using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ContinueButtonState : MonoBehaviour
{
    private void Start()
    {
        // Automatically grab the button component attached to this same GameObject
        Button continueButton = GetComponent<Button>();
        
        if (continueButton != null)
        {
            // If any save exists, the button is interactable. Otherwise, it is disabled.
            continueButton.interactable = SaveManager.AnySaveExists();
        }
    }
}
