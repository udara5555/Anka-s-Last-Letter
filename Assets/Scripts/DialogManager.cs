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
        nameText.text = currentDialog.characterName;
        
        if (characterImage != null && currentDialog.characterSprite != null)
        {
            characterImage.sprite = currentDialog.characterSprite;
        }
        
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Don't do anything if no dialog is active
        if (currentDialog == null) return;

        if (currentSentenceIndex < currentDialog.sentences.Length)
        {
            dialogText.text = currentDialog.sentences[currentSentenceIndex];
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
