using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using System.Threading.Tasks;
 
public class PortraitDialogueUI : DialoguePresenterBase
{
    [Header("Portrait Settings")]
    public Image portraitImage; // assign in inspector
    public Sprite defaultPortrait;

    [System.Serializable]
    public class CharacterPortrait
    {
        public string characterName;
        public Sprite portrait;
    }

    public List<CharacterPortrait> portraits = new List<CharacterPortrait>();

    private Dictionary<string, Sprite> portraitDict;

    void Awake()
    {
        portraitDict = new Dictionary<string, Sprite>();
        foreach (var entry in portraits)
        {
            if (!portraitDict.ContainsKey(entry.characterName))
            {
                portraitDict.Add(entry.characterName, entry.portrait);
            }
        }
    }


    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        await Task.Delay(0);

        string speaker = line.CharacterName;


       // string text = line.Text.Text;

        //Debug.Log($"Speaker: {speaker}");

        if (line.CharacterName != null)
        {
            //Debug.Log(line.CharacterName);
            portraitImage.sprite = portraitDict[line.CharacterName];
            portraitImage.preserveAspect = true;
        }
        else
        {
            portraitImage.sprite = defaultPortrait;
        }

        // You must still pass the line to another view (e.g. LineView) to show text.
        // Example: you could reference a LineView and call it here.

         





    }

    public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, CancellationToken cancellationToken)
    {
        // Implement option handling
        await Task.Delay(0);
        return null;
    }

    public override async YarnTask OnDialogueStartedAsync()
    {
        await Task.Delay(0);
        //Debug.Log("Dialogue started");
    }

    public override async YarnTask OnDialogueCompleteAsync()
    {
        await Task.Delay(0);
        //Debug.Log("Dialogue complete");
    }























   /* public override void RunLine(LocalizedLine dialogueLine, System.Action onDialogueLineFinished)
    {
        // Set portrait based on character
        if (dialogueLine.CharacterName != null && portraitDict.ContainsKey(dialogueLine.CharacterName))
        {
            Debug.Log(dialogueLine.CharacterName);
            portraitImage.sprite = portraitDict[dialogueLine.CharacterName];
        }
        else
        {
            portraitImage.sprite = defaultPortrait;
        }

        // You must still pass the line to another view (e.g. LineView) to show text.
        // Example: you could reference a LineView and call it here.

        onDialogueLineFinished?.Invoke();
    }*/
}
