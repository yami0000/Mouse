using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Active : MonoBehaviour
{
    // The UI_Active page that is open right now. Only one can be open at a time.
    public static UI_Active Current { get; private set; }

    [Tooltip("Unique ID used by UI_Active.Open(\"id\") and the <<open_ui \"id\">> Yarn command.")]
    public string uiID;

    [SerializeField] GameObject uiParent;
    [SerializeField] private KeyCode triggerKey ;
    [Tooltip("Off for a page that uses the right mouse button itself, e.g. the skill page. Escape and the Trigger Key still close it.")]
    [SerializeField] private bool closeOnRightClick = true;

    private bool isMenuOpen;

    public bool IsOpen => isMenuOpen;
    public KeyCode TriggerKey => triggerKey;

    private void Start()
    {
        uiParent.SetActive(false);
    }

    private void OnDisable()
    {
        if (Current == this)
            Current = null;
    }

    private void Update()
    {
        if (Input.GetKeyUp(triggerKey))
        {
            // Same key toggles the page.
            if (isMenuOpen)
                CloseMenu();
            // Don't open over another page (shop, other UI_Active, death screen...).
            else if (!GM.Instance.GameManager.isUIOpened)
                OpenMenu();

            return;
        }

        if (!isMenuOpen) return;
        if (Input.GetKeyUp(KeyCode.Escape) || (closeOnRightClick && Input.GetKeyUp(KeyCode.Mouse1)))
            CloseMenu();
    }

    // Opening from code always wins: any other open UI_Active page is closed first.
    public void OpenMenu()
    {
        if (isMenuOpen) return;

        if (Current != null)
            Current.CloseMenu();

        isMenuOpen = true;
        Current = this;

        uiParent.SetActive(true);
        GM.Instance.GameManager.isUIOpened = true;
    }

    public void CloseMenu()
    {
        if (!isMenuOpen) return;

        isMenuOpen = false;
        if (Current == this)
            Current = null;

        uiParent.SetActive(false);
        GM.Instance.GameManager.isUIOpened = false;
    }

    /// <summary>
    /// Opens the UI_Active page with this ID in the current scene.
    /// Returns false and logs a warning if not found.
    /// </summary>
    public static bool Open(string id)
    {
        UI_Active ui = Find(id);
        if (ui == null) return false;

        ui.OpenMenu();
        return true;
    }

    public static UI_Active Find(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (var ui in FindObjectsByType<UI_Active>(FindObjectsSortMode.None))
            if (ui.uiID == id)
                return ui;

        Debug.LogWarning($"[UI_Active] No UI with ID '{id}' found in scene " +
                         $"'{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}'.");
        return null;
    }
}
