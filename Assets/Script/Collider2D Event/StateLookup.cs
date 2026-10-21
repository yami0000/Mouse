using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// Finds the states of any enemy / NPC (or anything else built the same way) without knowing its
/// type: its state machine is the member whose type has a ChangeState(state) method, and its states
/// are the members holding that state type (idleState, battleState...).
/// Used by ForceStateAction at runtime and by its drawer to list the states in the inspector.
/// </summary>
public static class StateLookup
{
    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static readonly Dictionary<Type, List<string>> namesCache = new Dictionary<Type, List<string>>();

    // CurrentState runs every frame for StateFX, so the reflection behind it is looked up once per type.
    private static readonly Dictionary<Type, (MemberInfo member, Type stateType)> machineCache = new Dictionary<Type, (MemberInfo, Type)>();
    private static readonly Dictionary<Type, PropertyInfo> currentStateCache = new Dictionary<Type, PropertyInfo>();

    /// <summary>Names of the state members of _entityType, base class ones first. Empty if it has no state machine.</summary>
    public static List<string> StateNames(Type _entityType)
    {
        if (namesCache.TryGetValue(_entityType, out List<string> cached))
            return cached;

        var names = new List<string>();
        MemberInfo machine = FindMachine(_entityType, out Type stateType);

        if (machine != null)
        {
            var hierarchy = new List<Type>();
            for (Type t = _entityType; t != null && t != typeof(object); t = t.BaseType)
                hierarchy.Insert(0, t);

            foreach (Type t in hierarchy)
            {
                // Backing fields of auto properties have '<' in their name; the property itself is listed instead.
                foreach (FieldInfo field in t.GetFields(Members))
                    if (!field.Name.Contains("<") && stateType.IsAssignableFrom(field.FieldType) && !names.Contains(field.Name))
                        names.Add(field.Name);

                foreach (PropertyInfo property in t.GetProperties(Members))
                    if (property.CanRead && property.GetIndexParameters().Length == 0 &&
                        stateType.IsAssignableFrom(property.PropertyType) && !names.Contains(property.Name))
                        names.Add(property.Name);
            }
        }

        namesCache[_entityType] = names;
        return names;
    }

    /// <summary>The state object _entity holds under _stateName, or null.</summary>
    public static object GetState(object _entity, string _stateName)
    {
        if (_entity == null || string.IsNullOrEmpty(_stateName)) return null;
        return GetValue(FindMember(_entity.GetType(), _stateName), _entity);
    }

    /// <summary>The state _entity is in right now, or null.</summary>
    public static object CurrentState(object _entity)
    {
        object machine = Machine(_entity);
        if (machine == null) return null;

        Type machineType = machine.GetType();
        if (!currentStateCache.TryGetValue(machineType, out PropertyInfo current))
        {
            current = machineType.GetProperty("currentState", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            currentStateCache[machineType] = current;
        }

        return current?.GetValue(machine);
    }

    /// <summary>Puts _entity into _stateName (leaving its current state first). False if it has no such state.</summary>
    public static bool Enter(object _entity, string _stateName)
    {
        object machine = Machine(_entity);
        object state = GetState(_entity, _stateName);

        if (machine == null || state == null)
            return false;

        // Not started yet (its Start hasn't run): there is no state to leave, so start it in this one.
        string method = CurrentState(_entity) == null ? "Initialize" : "ChangeState";
        MethodInfo call = machine.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public);

        if (call == null)
            return false;

        call.Invoke(machine, new[] { state });
        return true;
    }

    private static object Machine(object _entity)
    {
        return _entity == null ? null : GetValue(FindMachine(_entity.GetType(), out _), _entity);
    }

    /// <summary>The member holding the state machine: the first one whose type has ChangeState(x) with one parameter.</summary>
    private static MemberInfo FindMachine(Type _entityType, out Type _stateType)
    {
        if (!machineCache.TryGetValue(_entityType, out var found))
        {
            MemberInfo member = SearchMachine(_entityType, out Type stateType);
            found = (member, stateType);
            machineCache[_entityType] = found;
        }

        _stateType = found.stateType;
        return found.member;
    }

    private static MemberInfo SearchMachine(Type _entityType, out Type _stateType)
    {
        for (Type t = _entityType; t != null && t != typeof(object); t = t.BaseType)
        {
            foreach (MemberInfo member in t.GetMembers(Members))
            {
                Type memberType = member is PropertyInfo p ? p.PropertyType : member is FieldInfo f ? f.FieldType : null;
                if (memberType == null || memberType.IsPrimitive || memberType == typeof(string)) continue;

                MethodInfo change = memberType.GetMethod("ChangeState", BindingFlags.Instance | BindingFlags.Public);
                ParameterInfo[] parameters = change?.GetParameters();

                if (parameters != null && parameters.Length == 1)
                {
                    _stateType = parameters[0].ParameterType;
                    return member;
                }
            }
        }

        _stateType = null;
        return null;
    }

    private static MemberInfo FindMember(Type _type, string _name)
    {
        for (Type t = _type; t != null && t != typeof(object); t = t.BaseType)
        {
            MemberInfo member = (MemberInfo)t.GetProperty(_name, Members) ?? t.GetField(_name, Members);
            if (member != null) return member;
        }

        return null;
    }

    private static object GetValue(MemberInfo _member, object _target)
    {
        switch (_member)
        {
            case PropertyInfo property: return property.GetValue(_target);
            case FieldInfo field:       return field.GetValue(_target);
            default:                    return null;
        }
    }
}
