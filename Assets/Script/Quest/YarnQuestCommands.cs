using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using Yarn.Unity;

public class YarnQuestCommands : MonoBehaviour
{
    public DialogueRunner runner;

    private int pendingSceneIndex = -1;
    private string pendingSpawnId;

    void Awake()
    {
        runner.AddCommandHandler<string>("accept_quest", AcceptQuest);
        runner.AddCommandHandler<string>("progress_objective", ProgressObjective);
        runner.AddCommandHandler<string, int>("get_item", GetItem);
        runner.AddCommandHandler<string, int>("remove_item", RemoveItem);
        runner.AddCommandHandler<string>("open_shop", OpenShop);
        runner.AddCommandHandler<string>("open_ui", OpenUI);
        runner.AddCommandHandler<int, string>("change_scene", ChangeScene);

        runner.AddFunction<string, int, bool>("has_item", HasItem);
        runner.AddFunction<string, int>("item_count", GetItemCount);

        runner.onDialogueComplete?.AddListener(LoadPendingScene);
    }

    void AcceptQuest(string questID)
    {
        QuestData quest = QuestDatabase.Instance.GetQuest(questID);

        if (quest == null)
        {
            Debug.LogError("Quest not found: " + questID);
            return;
        }

        QuestManager.Instance.AcceptQuest(quest);
    }
    void ProgressObjective(string dialogueKey)
    {
        QuestManager.Instance.ProgressTalkObjective(dialogueKey);
    }

    // <<get_item "0002">> gives one, <<get_item "0002" 30>> gives 30 (one notification showing x30).
    void GetItem(string itemID, int amount = 1)
    {
        ItemData item = ItemDatabase.Instance.GetItem(itemID);

        if (item == null)
        {
            Debug.LogError("Item not found: " + itemID);
            return;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"get_item '{itemID}': amount must be at least 1 (got {amount}).");
            return;
        }

        Inventory.Instance.AddItem(item, amount);
    }

    void RemoveItem(string itemID, int amount = 1)
    {
        ItemData item = ItemDatabase.Instance.GetItem(itemID);

        if (item == null)
        {
            Debug.LogError("Item not found: " + itemID);
            return;
        }

        if (GetItemCount(itemID) < amount)
        {
            Debug.LogWarning($"Not enough '{itemID}' to remove {amount}. Nothing was removed.");
            return;
        }

        for (int i = 0; i < amount; i++)
            Inventory.Instance.RemoveItem(item);
    }

    // Returns IEnumerator so Yarn waits here until the shop is closed,
    // then continues with the lines written after <<open_shop>>.
    IEnumerator OpenShop(string merchantID)
    {
        MerchantStock merchant = MerchantStock.Find(merchantID);

        if (merchant == null)
        {
            Debug.LogError("Merchant not found: " + merchantID);
            yield break;
        }

        merchant.OpenShop();
        yield return new WaitWhile(() => UI_Shop.Instance.IsOpen);
    }
    
    // Same as OpenShop: dialogue waits until the UI page is closed.
    IEnumerator OpenUI(string uiID)
    {
        UI_Active ui = UI_Active.Find(uiID);

        if (ui == null)
        {
            Debug.LogError("UI not found: " + uiID);
            yield break;
        }

        ui.OpenMenu();
        yield return new WaitWhile(() => ui.IsOpen);
    }

    // Counts the item in both the inventory and the stash
    int GetItemCount(string itemID)
    {
        ItemData item = ItemDatabase.Instance.GetItem(itemID);

        if (item == null)
        {
            Debug.LogError("Item not found: " + itemID);
            return 0;
        }

        // Equipment is held as a runtime copy, so ask the inventory rather than looking the asset up directly.
        return Inventory.Instance.GetStackSize(item);
    }

    bool HasItem(string itemID, int amount)
    {
        return GetItemCount(itemID) >= amount;
    }

    // The scene is loaded after the dialogue ends, so the dialogue UI isn't destroyed mid-line
    void ChangeScene(int sceneIndex, string spawnId = null)
    {
        pendingSceneIndex = sceneIndex;
        pendingSpawnId = spawnId;
    }

    void LoadPendingScene()
    {
        if (pendingSceneIndex < 0) return;

        int index = pendingSceneIndex;
        string spawnId = pendingSpawnId;
        pendingSceneIndex = -1;
        pendingSpawnId = null;

        GameManager.Instance.LoadScene(index, spawnId);
    }
}