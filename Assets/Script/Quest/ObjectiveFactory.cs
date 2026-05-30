using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectiveFactory
{
    public static QuestObjective Create(ObjectiveData data, Quest quest)
    {
        switch (data.type)
        {
            case ObjectiveType.KillEnemy:
                return new KillObjective(data, quest);

            case ObjectiveType.CollectItem:
                return new CollectObjective(data, quest);

            case ObjectiveType.TalkToNPC:
                return new TalkToNPCObjective(data, quest);

            default:
                return null;
        }
    }
}
