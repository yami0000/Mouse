using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestWorldEvent : MonoBehaviour
{
    [SerializeField] private string objectiveKey;
    [SerializeField] private string questID;

    // ...spawn/destroy fields same as before...

    void OnEnable()
    {
        GameEvents.OnObjectiveCompleted += HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;

        // NEW ¡ª apply immediately if already completed before this scene loaded
        if (!string.IsNullOrEmpty(objectiveKey) &&
            QuestWorldState.Instance.IsObjectiveCompleted(objectiveKey))
            ApplyObjectiveConsequences();

        if (!string.IsNullOrEmpty(questID) &&
            QuestWorldState.Instance.IsQuestCompleted(questID))
            ApplyQuestConsequences();
    }

    void OnDisable()
    {
        GameEvents.OnObjectiveCompleted -= HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
    }

    void HandleObjectiveCompleted(string key)
    {
        if (string.IsNullOrEmpty(objectiveKey) || key != objectiveKey) return;
        ApplyObjectiveConsequences();
    }

    void HandleQuestCompleted(string id)
    {
        if (string.IsNullOrEmpty(questID) || id != questID) return;
        ApplyQuestConsequences();
    }

    void ApplyObjectiveConsequences()
    {
        if (objectivePrefabToSpawn != null)
            Instantiate(objectivePrefabToSpawn, objectiveSpawnPoint.position, Quaternion.identity);
        if (objectiveObjectToDestroy != null)
            Destroy(objectiveObjectToDestroy);
    }

    void ApplyQuestConsequences()
    {
        if (questPrefabToSpawn != null)
            Instantiate(questPrefabToSpawn, questSpawnPoint.position, Quaternion.identity);
        if (questObjectToDestroy != null)
            Destroy(questObjectToDestroy);
    }
}
