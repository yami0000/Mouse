using UnityEditor;

/// <summary>
/// A Mod's Effects entries as a type dropdown plus that type's settings (see SerializeReferencePickerDrawer).
/// Every non-abstract ModEffect subclass is listed, so a new mechanism needs nothing here.
/// </summary>
[CustomPropertyDrawer(typeof(ModEffect), true)]
public class ModEffectPickerDrawer : SerializeReferencePickerDrawer { }
