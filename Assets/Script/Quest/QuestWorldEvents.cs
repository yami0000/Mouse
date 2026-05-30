using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestWorldEvents : MonoBehaviour
{
    [Header("Trigger Keys (leave empty to ignore)")]
    [SerializeField] private string objectiveKey; // matches ObjectiveData.dialogueKey
    [SerializeField] private string questID;      // matches QuestData.questID

    [Header("On Objective Complete")]
    [SerializeField] private GameObject objectivePrefabToSpawn;
    [SerializeField] private Transform objectiveSpawnPoint;
    [SerializeField] private GameObject objectiveObjectToDestroy;

    [Header("On Quest Complete")]
    [SerializeField] private GameObject questPrefabToSpawn;
    [SerializeField] private Transform questSpawnPoint;
    [SerializeField] private GameObject questObjectToDestroy;

    void OnEnable()
    {
        GameEvents.OnObjectiveCompleted += HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted += HandleQuestCompleted;
    }

    void OnDisable()
    {
        GameEvents.OnObjectiveCompleted -= HandleObjectiveCompleted;
        GameEvents.OnQuestCompleted -= HandleQuestCompleted;
    }

    void HandleObjectiveCompleted(string key)
    {
        if (string.IsNullOrEmpty(objectiveKey) || key != objectiveKey) return;

        if (objectivePrefabToSpawn != null)
            Instantiate(objectivePrefabToSpawn, objectiveSpawnPoint.position, Quaternion.identity);

        if (objectiveObjectToDestroy != null)
            Destroy(objectiveObjectToDestroy);
    }

    void HandleQuestCompleted(string id)
    {
        if (string.IsNullOrEmpty(questID) || id != questID) return;

        if (questPrefabToSpawn != null)
            Instantiate(questPrefabToSpawn, questSpawnPoint.position, Quaternion.identity);

        if (questObjectToDestroy != null)
            Destroy(questObjectToDestroy);
    }
}
