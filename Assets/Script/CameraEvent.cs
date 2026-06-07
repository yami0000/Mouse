using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
   
    DisableCamera,
    EnterBattleMode,
    
}
public class CameraEvent : DETECTION
{
    [SerializeField] private string DialogueName;

    [Tooltip("Dialogue starts after t seconds")]
    [SerializeField] private float t;

    [SerializeField] private List<EventAction> actions = new List<EventAction>();
    private bool _hasTriggered = false;
    private Collider2D cd;
    [SerializeField]private Entity E;


    private void Start()
    {
       cd = GetComponent<Collider2D>(); 
       
    }
    public override void Event()
    {
        if (_hasTriggered) return;
        _hasTriggered = true;

        base.Event();
        CameraSetter.Instance.EnablePosition(this.transform);
        NarrativeManager.Instance.RequestDialogue(DialogueName, t, () => ExecuteActions());
        cd.enabled = false;
        
    }
    private void ExecuteActions()
    {
        foreach (var action in actions)
        {
            action.Execute(E);
        }
    }
}


[System.Serializable]
public class EventAction
{
    public ActionType actionType;

    public void Execute(Entity E = null)
    {
        switch (actionType)
        {
            case ActionType.DisableCamera:
                CameraSetter.Instance.DisablePosition();
                break;

            case ActionType.EnterBattleMode:
                Enemy enemy = E as Enemy;
                enemy.BattleState = true;
                break;
        }
    } 
}
