using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    // Singleton pattern so any script can easily find the manager
    public static DialogManager Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("The parent panel for the dialog")]
    public GameObject dialogPanel;
    
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogText;
    public Image characterImage;

    [Header("Character Database")]
    [Tooltip("Define all characters in your game here once!")]
    public CharacterData[] characterDatabase;

    private DialogData currentDialog;
    private int currentSentenceIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ensure the panel is hidden when the game starts
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(false);
        }
    }

    public void StartDialog(DialogData dialogData)
    {
        currentDialog = dialogData;
        currentSentenceIndex = 0;
        
        dialogPanel.SetActive(true);
        
        // Display the first sentence immediately
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Don't do anything if no dialog is active
        if (currentDialog == null) return;

        if (currentSentenceIndex < currentDialog.dialogueLines.Length)
        {
            // Grab the current line of dialogue
            DialogLine currentLine = currentDialog.dialogueLines[currentSentenceIndex];

            // Set the name and text
            nameText.text = currentLine.characterName;
            dialogText.text = currentLine.sentence;
            
            // Look up the matching sprite in our central database!
            if (characterImage != null)
            {
                bool spriteFound = false;
                foreach (CharacterData character in characterDatabase)
                {
                    if (character.characterName == currentLine.characterName)
                    {
                        characterImage.sprite = character.characterSprite;
                        spriteFound = true;
                        break;
                    }
                }
                
                // Optional: clear the image if they typed a name wrong or left it blank
                if (!spriteFound)
                {
                    characterImage.sprite = null; 
                }
            }

            // Move to the next index for the next click
            currentSentenceIndex++;
        }
        else
        {
            EndDialog();
        }
    }

    private void EndDialog()
    {
        dialogPanel.SetActive(false);
        currentDialog = null;
    }
}
