using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Tools > Skill Tree > Set Up Scene: builds the new skill page in the open scene and carries over the
/// old tree's skills, positions and connections. One undo step, and nothing is saved until you save the scene.
/// Only needed once, on the scene that has the old page (_bootstrap).
/// </summary>
public static class SkillTreeSetup
{
    private const float ActiveRowSize = 120f;
    private const float HudRowSize = 96f;

    [MenuItem("Tools/Skill Tree/Set Up Scene")]
    private static void SetUpMenu()
    {
        Skill skill = Object.FindFirstObjectByType<Skill>(FindObjectsInactive.Include);

        if (skill == null)
        {
            EditorUtility.DisplayDialog("Skill Tree", "No Skill component in the open scene. Open _bootstrap first.", "OK");
            return;
        }

        if (skill.GetComponent<SkillTree>() != null)
        {
            EditorUtility.DisplayDialog("Skill Tree", "This scene already has the new skill tree.", "OK");
            return;
        }

        SetUp(skill);
        Debug.Log("[SkillTree] Set up. Check the skill page, then save the scene and delete Icon.cs.", skill);
    }

    [MenuItem("Tools/Skill Tree/Check for Problems")]
    private static void CheckMenu()
    {
        SkillTree tree = Object.FindFirstObjectByType<SkillTree>(FindObjectsInactive.Include);

        if (tree != null)
            SkillTreeAuthoring.LogProblems(tree);
        else
            Debug.LogWarning("[SkillTree] No SkillTree in the open scene.");
    }

    public static void SetUp(Skill _skill)
    {
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Set Up Skill Tree");

        UI_Active page = _skill.GetComponent<UI_Active>();
        SerializedObject pageSettings = new SerializedObject(page);
        Transform uiParent = ((GameObject)pageSettings.FindProperty("uiParent").objectReferenceValue).transform;
        Transform background = uiParent.Find("BG") != null ? uiParent.Find("BG") : uiParent;
        TMP_FontAsset font = FindFont(background);

        // The right mouse button installs cyberware on this page, so it can't close it as well.
        pageSettings.FindProperty("closeOnRightClick").boolValue = false;
        pageSettings.ApplyModifiedProperties();

        (RectTransform content, SkillLinks links) = BuildMap(background);

        SkillDescription description = Undo.AddComponent<SkillDescription>(_skill.gameObject);
        UI_CyberwarePanel panel = BuildCyberwarePanel(background, font);

        SkillTree tree = Undo.AddComponent<SkillTree>(_skill.gameObject);
        Sprite defaultShape = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        SerializedObject treeSettings = new SerializedObject(tree);
        treeSettings.FindProperty("jointRoot").objectReferenceValue = content;
        treeSettings.FindProperty("links").objectReferenceValue = links;
        treeSettings.FindProperty("description").objectReferenceValue = description;
        treeSettings.FindProperty("cyberwarePanel").objectReferenceValue = panel;

        SerializedProperty looks = treeSettings.FindProperty("looks");
        for (int i = 0; i < looks.arraySize; i++)
            looks.GetArrayElementAtIndex(i).FindPropertyRelative("shape").objectReferenceValue = defaultShape;

        treeSettings.ApplyModifiedProperties();

        MigrateOldJoints(tree, background);
        RemoveOldPage(background);

        BuildActiveSkillList(background, font, defaultShape);
        BuildSkillPoints(background, font);
        panel.transform.SetAsLastSibling();

        ReplaceOldHud(_skill.transform.parent, defaultShape, font);

        // SK, DragManager and anything else whose script is gone.
        Undo.RegisterCompleteObjectUndo(_skill.gameObject, "Set Up Skill Tree");
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(_skill.gameObject);

        SkillTreeAuthoring.RefreshPreview(tree);
        EditorSceneManager.MarkSceneDirty(_skill.gameObject.scene);
        Undo.CollapseUndoOperations(undoGroup);
    }

    #region The map

    private static (RectTransform, SkillLinks) BuildMap(Transform _background)
    {
        RectTransform viewRect = NewChild("Skill Map", _background);
        Stretch(viewRect, 300f, 40f, 40f, 40f);

        // See-through, but it catches the drags and wheel over the empty parts of the map.
        Image catcher = Undo.AddComponent<Image>(viewRect.gameObject);
        catcher.color = new Color(0f, 0f, 0f, 0f);

        RectMask2D mask = Undo.AddComponent<RectMask2D>(viewRect.gameObject);
        mask.enabled = false; // UI_SkillMapView turns it on in play; off, the whole map shows while editing

        RectTransform content = NewChild("Joints", viewRect);
        content.anchorMin = content.anchorMax = content.pivot = Vector2.one * 0.5f;
        content.sizeDelta = Vector2.one * 4000f;
        content.anchoredPosition = Vector2.zero;

        RectTransform linksRect = NewChild("Links", content);
        Stretch(linksRect, 0f, 0f, 0f, 0f);
        SkillLinks links = Undo.AddComponent<SkillLinks>(linksRect.gameObject);
        links.raycastTarget = false;

        UI_SkillMapView view = Undo.AddComponent<UI_SkillMapView>(viewRect.gameObject);
        SerializedObject settings = new SerializedObject(view);
        settings.FindProperty("content").objectReferenceValue = content;
        settings.FindProperty("mask").objectReferenceValue = mask;
        settings.ApplyModifiedProperties();

        return (content, links);
    }

    private static void MigrateOldJoints(SkillTree _tree, Transform _background)
    {
        var made = new Dictionary<Icon, SkillJoint>();
        Icon[] icons = _background.GetComponentsInChildren<Icon>(true);

        foreach (Icon icon in icons)
        {
            SerializedObject old = new SerializedObject(icon);
            string skillName = old.FindProperty("skillName").stringValue;
            RectTransform oldRect = (RectTransform)icon.transform;

            SkillJoint joint = SkillTreeAuthoring.CreateJoint(_tree, SkillJointKind.Basic, oldRect.anchoredPosition);
            joint.name = skillName;

            RectTransform rect = (RectTransform)joint.transform;
            rect.sizeDelta = oldRect.sizeDelta;
            rect.localScale = oldRect.localScale;

            joint.info.skillName = skillName;
            joint.info.description = old.FindProperty("skillDescription").stringValue;
            joint.info.icon = icon.normalSprite;
            joint.info.activeIcon = icon.activeSprite;
            joint.info.cost = old.FindProperty("cost").intValue;

            ApplyOldEffects(joint, skillName);
            made[icon] = joint;
        }

        foreach (Icon icon in icons)
        {
            SerializedProperty required = new SerializedObject(icon).FindProperty("RequiredSkill");

            for (int i = 0; i < required.arraySize; i++)
                if (required.GetArrayElementAtIndex(i).objectReferenceValue is Icon earlier && made.TryGetValue(earlier, out SkillJoint previous))
                    made[icon].previous.Add(previous);
        }
    }

    // These effects were string comparisons in the old Skill.cs. The cooldowns are the old Skill component's values.
    private static void ApplyOldEffects(SkillJoint _joint, string _skillName)
    {
        SkillInfo info = _joint.info;
        _joint.kind = SkillJointKind.Special;

        switch (_skillName)
        {
            case "Metabolism": info.mechanism = SkillMechanism.Metabolism; break;
            case "OverCharging": info.mechanism = SkillMechanism.OverCharging; break;
            case "Double Jump": info.mechanism = SkillMechanism.DoubleJump; break;

            case "FirePower Training":
                _joint.kind = SkillJointKind.Basic;
                info.stats.Add(new SkillStatBonus { stat = SkillStat.Damage, value = 5 });
                break;

            case "Defense Training":
                _joint.kind = SkillJointKind.Basic;
                info.stats.Add(new SkillStatBonus { stat = SkillStat.Armor, value = 10 });
                break;

            case "Protective Shield":
                info.activeSkill = ActiveSkillId.ProtectiveShield;
                info.cooldown = 6f;
                info.defaultKey = KeyCode.Mouse1; // what it was on before
                break;

            case "Reaper":
                info.activeSkill = ActiveSkillId.Reaper;
                info.cooldown = 35f;
                info.defaultKey = KeyCode.R; // both used to share the right button
                break;

            default:
                _joint.kind = SkillJointKind.Basic;
                Debug.LogWarning($"[SkillTree] Don't know what the old skill \"{_skillName}\" did: it came across with no effect.", _joint);
                break;
        }
    }

    private static void RemoveOldPage(Transform _background)
    {
        foreach (Icon icon in _background.GetComponentsInChildren<Icon>(true))
        {
            if (icon == null) continue; // went with something removed before it

            // Each old joint knows its hand-placed line, wherever that was put.
            Object line = new SerializedObject(icon).FindProperty("line").objectReferenceValue;
            if (line != null)
                Undo.DestroyObjectImmediate(((RawImage)line).gameObject);

            if (icon != null)
                Undo.DestroyObjectImmediate(icon.gameObject);
        }

        foreach (string oldPart in new[] { "Slot", "Description", "Line" })
        {
            Transform part = _background.Find(oldPart);
            if (part != null)
                Undo.DestroyObjectImmediate(part.gameObject);
        }
    }

    #endregion

    #region Active skills and skill points

    private static void BuildActiveSkillList(Transform _background, TMP_FontAsset _font, Sprite _shape)
    {
        // Where the old single slot was, growing downwards.
        RectTransform list = NewChild("Active Skills", _background);
        list.anchorMin = list.anchorMax = Vector2.one * 0.5f;
        list.pivot = new Vector2(0.5f, 1f);
        list.anchoredPosition = new Vector2(-760f, 445f);
        list.sizeDelta = new Vector2(180f, 880f);

        AddStack(list, 12f);
        AddListBehaviour(list, BuildRow(list, ActiveRowSize, _shape, _font, true), true);
    }

    private static void BuildSkillPoints(Transform _background, TMP_FontAsset _font)
    {
        RectTransform rect = NewChild("Skill Points", _background);
        rect.anchorMin = rect.anchorMax = Vector2.one * 0.5f;
        rect.anchoredPosition = new Vector2(-760f, 490f);
        rect.sizeDelta = new Vector2(280f, 70f);

        TextMeshProUGUI text = AddText(rect, _font, 40f, TextAlignmentOptions.Center);
        text.text = "SP";
        Undo.AddComponent<UI_SkillPoints>(rect.gameObject);
    }

    private static void ReplaceOldHud(Transform _canvas, Sprite _shape, TMP_FontAsset _font)
    {
        Transform hud = _canvas != null ? _canvas.Find("UI_Health") : null;
        if (hud == null)
        {
            Debug.LogWarning("[SkillTree] No UI_Health found: add a HUD list for the cooldowns yourself.");
            return;
        }

        // Named "Skill" and "Skill  " in the scene.
        Transform oldIcon = hud.Find("Skill");
        Vector2 position = oldIcon != null ? ((RectTransform)oldIcon).anchoredPosition : Vector2.zero;
        Vector2 anchor = oldIcon != null ? ((RectTransform)oldIcon).anchorMin : Vector2.one * 0.5f;
        int siblingIndex = oldIcon != null ? oldIcon.GetSiblingIndex() : hud.childCount;

        var oldParts = new List<GameObject>();
        foreach (Transform child in hud)
            if (child.name.Trim() == "Skill")
                oldParts.Add(child.gameObject);

        foreach (GameObject part in oldParts)
            Undo.DestroyObjectImmediate(part);

        RectTransform list = NewChild("Active Skills", hud);
        list.SetSiblingIndex(Mathf.Min(siblingIndex, hud.childCount - 1));
        list.anchorMin = list.anchorMax = anchor;
        list.pivot = new Vector2(0.5f, 1f);
        list.anchoredPosition = position + Vector2.up * (HudRowSize * 0.5f);
        list.sizeDelta = new Vector2(HudRowSize + 20f, 600f);

        AddStack(list, 8f);
        AddListBehaviour(list, BuildRow(list, HudRowSize, _shape, _font, false), false);
    }

    private static void AddStack(RectTransform _list, float _spacing)
    {
        VerticalLayoutGroup stack = Undo.AddComponent<VerticalLayoutGroup>(_list.gameObject);
        stack.childAlignment = TextAnchor.UpperCenter;
        stack.spacing = _spacing;
        stack.childControlWidth = false;
        stack.childControlHeight = false;
        stack.childForceExpandWidth = false;
        stack.childForceExpandHeight = false;
    }

    private static void AddListBehaviour(RectTransform _list, UI_ActiveSkillRow _template, bool _interactive)
    {
        UI_ActiveSkillList list = Undo.AddComponent<UI_ActiveSkillList>(_list.gameObject);
        SerializedObject settings = new SerializedObject(list);
        settings.FindProperty("rowTemplate").objectReferenceValue = _template;
        settings.FindProperty("interactive").boolValue = _interactive;
        settings.ApplyModifiedProperties();
    }

    private static UI_ActiveSkillRow BuildRow(RectTransform _list, float _size, Sprite _shape, TMP_FontAsset _font, bool _hoverable)
    {
        RectTransform rect = NewChild("Row Template", _list);
        rect.sizeDelta = Vector2.one * _size;

        Image fill = Undo.AddComponent<Image>(rect.gameObject);
        fill.sprite = _shape;
        fill.raycastTarget = _hoverable;

        Image icon = AddImage(NewStretched("Icon", rect), false);
        icon.preserveAspect = true;

        Image cooldown = AddImage(NewStretched("Cooldown", rect), false);
        cooldown.sprite = _shape;
        cooldown.color = new Color(0f, 0f, 0f, 0.65f);
        cooldown.type = Image.Type.Filled;
        cooldown.fillMethod = Image.FillMethod.Radial360;
        cooldown.fillOrigin = (int)Image.Origin360.Top;
        cooldown.fillAmount = 0f;

        RectTransform keyRect = NewChild("Key", rect);
        keyRect.anchorMin = new Vector2(0.4f, 0f);
        keyRect.anchorMax = new Vector2(1f, 0.35f);
        keyRect.offsetMin = keyRect.offsetMax = Vector2.zero;
        TextMeshProUGUI key = AddText(keyRect, _font, _size * 0.2f, TextAlignmentOptions.BottomRight);

        UI_ActiveSkillRow row = Undo.AddComponent<UI_ActiveSkillRow>(rect.gameObject);
        SerializedObject settings = new SerializedObject(row);
        settings.FindProperty("fill").objectReferenceValue = fill;
        settings.FindProperty("icon").objectReferenceValue = icon;
        settings.FindProperty("cooldown").objectReferenceValue = cooldown;
        settings.FindProperty("keyLabel").objectReferenceValue = key;
        settings.ApplyModifiedProperties();

        rect.gameObject.SetActive(false);
        return row;
    }

    #endregion

    #region The cyberware panel

    private static UI_CyberwarePanel BuildCyberwarePanel(Transform _background, TMP_FontAsset _font)
    {
        RectTransform root = NewChild("Cyberware Panel", _background);
        root.anchorMin = root.anchorMax = Vector2.one * 0.5f;
        root.anchoredPosition = new Vector2(660f, 0f);
        root.sizeDelta = new Vector2(480f, 820f);

        Image backdrop = Undo.AddComponent<Image>(root.gameObject);
        backdrop.color = new Color(0.06f, 0.07f, 0.1f, 0.95f);

        TextMeshProUGUI title = AddLabel(root, "Title", _font, 36f, 0f, 70f, "");
        AddLabel(root, "Installed Label", _font, 24f, 80f, 40f, "Installed  (right-click to remove)");

        UI_CyberwareSlot installed = BuildSlot(root, _font, "Installed Slot");
        RectTransform installedRect = (RectTransform)installed.transform;
        installedRect.anchorMin = installedRect.anchorMax = new Vector2(0.5f, 1f);
        installedRect.pivot = new Vector2(0.5f, 1f);
        installedRect.anchoredPosition = new Vector2(0f, -125f);

        AddLabel(root, "Carried Label", _font, 24f, 260f, 40f, "Carried  (right-click to install)");

        RectTransform grid = NewChild("Carried", root);
        grid.anchorMin = Vector2.zero;
        grid.anchorMax = Vector2.one;
        grid.offsetMin = new Vector2(24f, 24f);
        grid.offsetMax = new Vector2(-24f, -305f);

        GridLayoutGroup layout = Undo.AddComponent<GridLayoutGroup>(grid.gameObject);
        layout.cellSize = Vector2.one * 100f;
        layout.spacing = Vector2.one * 12f;

        UI_CyberwareSlot template = BuildSlot(grid, _font, "Slot Template");
        template.gameObject.SetActive(false);

        TextMeshProUGUI empty = AddLabel(root, "Nothing Carried", _font, 24f, 320f, 40f, "No cyberware to install");

        UI_CyberwarePanel panel = Undo.AddComponent<UI_CyberwarePanel>(root.gameObject);
        SerializedObject settings = new SerializedObject(panel);
        settings.FindProperty("title").objectReferenceValue = title;
        settings.FindProperty("installedSlot").objectReferenceValue = installed;
        settings.FindProperty("slotTemplate").objectReferenceValue = template;
        settings.FindProperty("emptyMessage").objectReferenceValue = empty.gameObject;
        settings.ApplyModifiedProperties();

        root.gameObject.SetActive(false);
        return panel;
    }

    private static UI_CyberwareSlot BuildSlot(RectTransform _parent, TMP_FontAsset _font, string _name)
    {
        RectTransform rect = NewChild(_name, _parent);
        rect.sizeDelta = Vector2.one * 120f;

        Image backdrop = Undo.AddComponent<Image>(rect.gameObject);
        backdrop.color = new Color(1f, 1f, 1f, 0.08f);

        Image icon = AddImage(NewStretched("Icon", rect), false);
        icon.preserveAspect = true;

        RectTransform countRect = NewStretched("Count", rect);
        TextMeshProUGUI count = AddText(countRect, _font, 22f, TextAlignmentOptions.BottomRight);

        UI_CyberwareSlot slot = Undo.AddComponent<UI_CyberwareSlot>(rect.gameObject);
        SerializedObject settings = new SerializedObject(slot);
        settings.FindProperty("icon").objectReferenceValue = icon;
        settings.FindProperty("count").objectReferenceValue = count;
        settings.ApplyModifiedProperties();

        return slot;
    }

    private static TextMeshProUGUI AddLabel(RectTransform _parent, string _name, TMP_FontAsset _font, float _size, float _fromTop, float _height, string _text)
    {
        RectTransform rect = NewChild(_name, _parent);
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 1f);
        rect.offsetMin = new Vector2(24f, -_fromTop - _height);
        rect.offsetMax = new Vector2(-24f, -_fromTop);

        TextMeshProUGUI label = AddText(rect, _font, _size, TextAlignmentOptions.MidlineLeft);
        label.text = _text;
        return label;
    }

    #endregion

    #region Building blocks

    private static RectTransform NewChild(string _name, Transform _parent)
    {
        GameObject go = new GameObject(_name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Set Up Skill Tree");
        go.layer = _parent.gameObject.layer;
        go.transform.SetParent(_parent, false);
        return (RectTransform)go.transform;
    }

    private static RectTransform NewStretched(string _name, Transform _parent)
    {
        RectTransform rect = NewChild(_name, _parent);
        Stretch(rect, 0f, 0f, 0f, 0f);
        return rect;
    }

    private static void Stretch(RectTransform _rect, float _left, float _right, float _top, float _bottom)
    {
        _rect.anchorMin = Vector2.zero;
        _rect.anchorMax = Vector2.one;
        _rect.offsetMin = new Vector2(_left, _bottom);
        _rect.offsetMax = new Vector2(-_right, -_top);
    }

    private static Image AddImage(RectTransform _rect, bool _raycast)
    {
        Image image = Undo.AddComponent<Image>(_rect.gameObject);
        image.raycastTarget = _raycast;
        return image;
    }

    private static TextMeshProUGUI AddText(RectTransform _rect, TMP_FontAsset _font, float _size, TextAlignmentOptions _alignment)
    {
        TextMeshProUGUI text = Undo.AddComponent<TextMeshProUGUI>(_rect.gameObject);
        if (_font != null) text.font = _font;
        text.fontSize = _size;
        text.alignment = _alignment;
        text.raycastTarget = false;
        return text;
    }

    // The font the old page's text was using, so the new parts match.
    private static TMP_FontAsset FindFont(Transform _background)
    {
        TextMeshProUGUI existing = _background.GetComponentInChildren<TextMeshProUGUI>(true);
        return existing != null ? existing.font : TMP_Settings.defaultFontAsset;
    }

    #endregion
}
