using Yarn.Unity;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class TEST : DialoguePresenterBase
{
    public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
      
        string speaker = line.CharacterName;

      
       // string text = line.Text.Text;

        Debug.Log($"Speaker: {speaker}");

  
        await Task.Delay(0);

        
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
        Debug.Log("Dialogue started");
    }

    public override async YarnTask OnDialogueCompleteAsync()
    {
        await Task.Delay(0);
        Debug.Log("Dialogue complete");
    }
}
