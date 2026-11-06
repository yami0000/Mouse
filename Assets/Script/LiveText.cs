using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// A text field that can show values from its object, written as {tokens} (see LiveText).
/// Drawn as a text area with a live preview and an Insert Value menu (Editor/LiveTextDrawer).
/// </summary>
public class LiveTextAttribute : PropertyAttribute
{
    public readonly int minLines;

    public LiveTextAttribute(int _minLines = 3)
    {
        minLines = Mathf.Max(1, _minLines);
    }
}

/// <summary>How the values filled into a text look: TextMeshPro rich text put around each one.</summary>
[Serializable]
public struct LiveTextStyle
{
    [Tooltip("Color the filled-in values.")]
    public bool colorValues;
    public Color valueColor;
    [Tooltip("Make the filled-in values bold.")]
    public bool boldValues;

    public static LiveTextStyle Default => new LiveTextStyle
    {
        colorValues = true,
        valueColor = new Color(0.85f, 0.42f, 0f, 1f),
        boldValues = false
    };

    public static LiveTextStyle Plain => new LiveTextStyle();
}

/// <summary>
/// Fills the {tokens} in a text with values read from an object (an item, a mod...), so descriptions
/// always show the numbers the asset really has.
///
///   {damageRate}                      a field of the object itself (any field, private ones too; any case)
///   {EnhancedFirePowder.damageRate}   a field of one of its effects, named by type (the Mod_ is optional)
///   {Piercing[1].extraTargets}        the second effect of that type
///   {Weapon.Damage}                   the worn weapon's tooltip value (with its mods), or any of its fields
///   {Player.Damage}                   one of the player's stats (in game only)
///   {damageRate * Weapon.Damage}      maths: + - * / ( ), and round floor ceil abs min max
///   {damageRate:%}  {radius:0.0}      a C# number format after the colon; % alone = 0.##%
///   [[ ... ]]                         only shown when every value inside it can be shown
///   {{ and }}                         a plain { or }
/// </summary>
public static class LiveText
{
    public enum TokenState { Ok, NeedsGame, Broken }

    /// <summary>A token that couldn't be filled in, for the Inspector's checks.</summary>
    public struct Problem
    {
        public string token;
        public string message;
        public TokenState state;
    }

    /// <summary>Stands in for the worn weapon outside Play mode, so Weapon values can be previewed. Set from the Inspector.</summary>
    public static ItemData_Equipment previewWeapon;

    private const string DefaultNumberFormat = "0.##";

    /// <summary>_text with its tokens filled in from _root, for the game's UI.</summary>
    public static string Format(string _text, object _root) => Format(_text, _root, LiveTextStyle.Default);

    public static string Format(string _text, object _root, LiveTextStyle _style) =>
        Render(_text, _root, _style, false, null);

    /// <summary>
    /// For the Inspector: like Format, but values only the game has show as grey placeholders, broken tokens
    /// in red, and [[ ]] parts are always kept. Every token that couldn't be filled in goes into _problems.
    /// </summary>
    public static string Preview(string _text, object _root, LiveTextStyle _style, List<Problem> _problems) =>
        Render(_text, _root, _style, true, _problems);

    private static bool HasTokens(string _text) =>
        _text.IndexOf('{') >= 0 || _text.IndexOf('}') >= 0 || _text.IndexOf("[[", StringComparison.Ordinal) >= 0;

    private static string Render(string _text, object _root, LiveTextStyle _style, bool _preview, List<Problem> _problems)
    {
        if (string.IsNullOrEmpty(_text) || !HasTokens(_text))
            return _text;

        Context context = new Context { root = _root, style = _style, preview = _preview, problems = _problems };
        StringBuilder result = new StringBuilder(_text.Length + 32);
        bool missing = false;
        RenderPart(_text, 0, _text.Length, context, result, ref missing);
        return result.ToString();
    }

    private class Context
    {
        public object root;
        public LiveTextStyle style;
        public bool preview;
        public List<Problem> problems;

        public void Report(string _token, TokenState _state, string _message) =>
            problems?.Add(new Problem { token = _token, state = _state, message = _message });
    }

    // Appends _text[_start.._end) to _out with its tokens filled in. _missing: a value in it couldn't be shown.
    private static void RenderPart(string _text, int _start, int _end, Context _context, StringBuilder _out, ref bool _missing)
    {
        int i = _start;

        while (i < _end)
        {
            char c = _text[i];
            bool doubled = i + 1 < _end && _text[i + 1] == c;

            if ((c == '{' || c == '}') && doubled)
            {
                _out.Append(c);
                i += 2;
                continue;
            }

            if (c == '[' && doubled)
            {
                int close = _text.IndexOf("]]", i + 2, _end - i - 2, StringComparison.Ordinal);
                if (close >= 0)
                {
                    StringBuilder part = new StringBuilder();
                    bool partMissing = false;
                    RenderPart(_text, i + 2, close, _context, part, ref partMissing);

                    // In the Inspector it's kept, placeholders and all, so the author can see what it says.
                    if (!partMissing || _context.preview)
                        _out.Append(part);

                    i = close + 2;
                    continue;
                }
            }

            if (c == '{')
            {
                int close = _text.IndexOf('}', i + 1, _end - i - 1);
                if (close >= 0)
                {
                    AppendToken(_text.Substring(i + 1, close - i - 1), _context, _out, ref _missing);
                    i = close + 1;
                    continue;
                }

                _context.Report("{", TokenState.Broken, "A { has no } after it. Write {{ for a plain {.");
            }
            else if (c == '}')
            {
                _context.Report("}", TokenState.Broken, "A } has no { before it. Write }} for a plain }.");
            }

            _out.Append(c);
            i++;
        }
    }

    private static void AppendToken(string _token, Context _context, StringBuilder _out, ref bool _missing)
    {
        string expression = _token;
        string format = null;

        int colon = _token.LastIndexOf(':');
        if (colon >= 0)
        {
            expression = _token.Substring(0, colon);
            format = _token.Substring(colon + 1).Trim();
        }

        Result result = new Parser(expression, _context.root).Run();

        if (result.state == TokenState.Ok)
        {
            string text;
            try
            {
                text = result.ToText(format);
            }
            catch (FormatException)
            {
                result = Result.Broken($"\"{format}\" isn't a number format.");
                text = null;
            }

            if (text != null)
            {
                _out.Append(Styled(text, _context.style));
                return;
            }
        }

        _missing = true;
        _context.Report("{" + _token + "}", result.state, result.message);

        if (result.state == TokenState.NeedsGame)
            _out.Append(_context.preview ? $"<color=#888888>[{expression.Trim()}]</color>" : "?");
        else
            _out.Append(_context.preview ? $"<color=#FF4040>{{{_token}}}</color>" : "{" + _token + "}");
    }

    private static string Styled(string _value, LiveTextStyle _style)
    {
        if (_style.boldValues)
            _value = $"<b>{_value}</b>";
        if (_style.colorValues)
            _value = $"<color=#{ColorUtility.ToHtmlStringRGBA(_style.valueColor)}>{_value}</color>";
        return _value;
    }

    // ---------------------------------------------------------------- values

    private struct Result
    {
        public TokenState state;
        public bool isNumber;
        public double number;
        public string text;
        public string message;

        public static Result Number(double _number) => new Result { state = TokenState.Ok, isNumber = true, number = _number };
        public static Result Text(string _text) => new Result { state = TokenState.Ok, text = _text ?? "" };
        public static Result Broken(string _message) => new Result { state = TokenState.Broken, message = _message };
        public static Result NeedsGame(string _message) => new Result { state = TokenState.NeedsGame, message = _message };

        public string ToText(string _format)
        {
            if (!isNumber)
                return text;

            if (double.IsInfinity(number) || double.IsNaN(number))
                return "∞";

            string format = string.IsNullOrEmpty(_format) ? DefaultNumberFormat : _format == "%" ? "0.##%" : _format;
            return number.ToString(format, CultureInfo.InvariantCulture);
        }
    }

    // The worse of two results: broken beats needs-the-game beats fine.
    private static bool Failed(Result _a, Result _b, out Result _worse)
    {
        if (_a.state == TokenState.Broken) { _worse = _a; return true; }
        if (_b.state == TokenState.Broken) { _worse = _b; return true; }
        if (_a.state == TokenState.NeedsGame) { _worse = _a; return true; }
        if (_b.state == TokenState.NeedsGame) { _worse = _b; return true; }
        _worse = default;
        return false;
    }

    private static Result FromValue(object _value)
    {
        switch (_value)
        {
            case null: return Result.Text("");
            case Stats stats: return Result.Number(stats.GetValue());
            case bool flag: return Result.Number(flag ? 1 : 0);
            case string text: return Result.Text(text);
            case Enum named: return Result.Text(named.ToString());
            case LayerMask mask: return Result.Number(mask.value);
            case UnityEngine.Object asset: return Result.Text(asset != null ? asset.name : "None");
            case IConvertible number when IsNumeric(number):
                return Result.Number(Convert.ToDouble(number, CultureInfo.InvariantCulture));
            default: return Result.Text(_value.ToString());
        }
    }

    private static bool IsNumeric(IConvertible _value)
    {
        switch (_value.GetTypeCode())
        {
            case TypeCode.Byte: case TypeCode.SByte:
            case TypeCode.Int16: case TypeCode.UInt16:
            case TypeCode.Int32: case TypeCode.UInt32:
            case TypeCode.Int64: case TypeCode.UInt64:
            case TypeCode.Single: case TypeCode.Double: case TypeCode.Decimal:
                return true;
            default:
                return false;
        }
    }

    /// <summary>The worn weapon in game; the Inspector's Preview Weapon outside Play mode.</summary>
    public static ItemData_Equipment Weapon
    {
        get
        {
            if (!Application.isPlaying)
                return previewWeapon;

            return Inventory.Instance != null && Inventory.Instance.equipmentDictionary != null
                ? Inventory.Instance.GetEquipment(EquipmentType.Weapon)
                : null;
        }
    }

    private static PlayerStats Player
    {
        get
        {
            if (!Application.isPlaying)
                return null;

            PlayerManager manager = UnityEngine.Object.FindAnyObjectByType<PlayerManager>();
            return manager != null && manager.player != null ? manager.player.GetComponent<PlayerStats>() : null;
        }
    }

    // ---------------------------------------------------------------- members

    private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private static readonly Dictionary<(Type, string), MemberInfo> memberCache = new Dictionary<(Type, string), MemberInfo>();

    /// <summary>The field or property _name of _type or a base type: the exact name first, then ignoring case.</summary>
    public static MemberInfo FindMember(Type _type, string _name)
    {
        if (memberCache.TryGetValue((_type, _name), out MemberInfo cached))
            return cached;

        MemberInfo found = Find(_type, _name, StringComparison.Ordinal) ?? Find(_type, _name, StringComparison.OrdinalIgnoreCase);
        memberCache[(_type, _name)] = found;
        return found;
    }

    private static MemberInfo Find(Type _type, string _name, StringComparison _comparison)
    {
        for (Type type = _type; type != null && type != typeof(UnityEngine.Object) && type != typeof(object); type = type.BaseType)
        {
            foreach (FieldInfo field in type.GetFields(Members))
                if (!field.Name.StartsWith("<") && string.Equals(field.Name, _name, _comparison))
                    return field;

            foreach (PropertyInfo property in type.GetProperties(Members))
                if (property.CanRead && property.GetIndexParameters().Length == 0 && string.Equals(property.Name, _name, _comparison))
                    return property;
        }

        return null;
    }

    private static bool TryMember(object _owner, string _name, out object _value)
    {
        _value = null;
        if (_owner == null) return false;

        switch (FindMember(_owner.GetType(), _name))
        {
            case FieldInfo field:
                _value = field.GetValue(_owner);
                return true;
            case PropertyInfo property:
                try
                {
                    _value = property.GetValue(_owner);
                    return true;
                }
                catch (TargetInvocationException)
                {
                    return false;
                }
            default:
                return false;
        }
    }

    /// <summary>The name an effect goes by in tokens: its type name without a Mod_ or Effect_ in front.</summary>
    public static string ShortName(Type _type)
    {
        string name = _type.Name;
        if (name.StartsWith("Mod_")) return name.Substring(4);
        if (name.StartsWith("Effect_")) return name.Substring(7);
        return name;
    }

    private static bool IsNamed(Type _type, string _name) =>
        string.Equals(_type.Name, _name, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(ShortName(_type), _name, StringComparison.OrdinalIgnoreCase);

    /// <summary>The things in _root's lists (a mod's Effects, an item's Item Effects...), in order.</summary>
    public static List<object> ListedObjects(object _root)
    {
        List<object> objects = new List<object>();
        if (_root == null) return objects;

        for (Type type = _root.GetType(); type != null && type != typeof(UnityEngine.Object); type = type.BaseType)
            foreach (FieldInfo field in type.GetFields(Members))
            {
                if (field.FieldType == typeof(string) || !typeof(IList).IsAssignableFrom(field.FieldType)) continue;
                if (!(field.GetValue(_root) is IList list)) continue;

                foreach (object element in list)
                    if (element != null && !(element is UnityEngine.Object asset && asset == null) && !IsPlain(element))
                        objects.Add(element);
            }

        return objects;
    }

    // Numbers and strings in a list aren't effects.
    private static bool IsPlain(object _value) => _value is IConvertible;

    // ---------------------------------------------------------------- expressions

    private sealed class Parser
    {
        private readonly string s;
        private readonly object root;
        private int p;

        public Parser(string _text, object _root)
        {
            s = _text ?? "";
            root = _root;
        }

        public Result Run()
        {
            Skip();
            if (p >= s.Length)
                return Result.Broken("The token is empty.");

            Result result = Sum();

            Skip();
            if (p < s.Length && result.state != TokenState.Broken)
                return Result.Broken($"Didn't understand \"{s.Substring(p)}\".");

            return result;
        }

        private void Skip()
        {
            while (p < s.Length && char.IsWhiteSpace(s[p])) p++;
        }

        private bool Take(char _c)
        {
            Skip();
            if (p < s.Length && s[p] == _c)
            {
                p++;
                return true;
            }
            return false;
        }

        private Result Sum()
        {
            Result a = Product();

            while (true)
            {
                Skip();
                if (p >= s.Length || (s[p] != '+' && s[p] != '-')) return a;

                char op = s[p++];
                Result b = Product();
                a = Combine(a, b, op);
            }
        }

        private Result Product()
        {
            Result a = Unary();

            while (true)
            {
                Skip();
                if (p >= s.Length || (s[p] != '*' && s[p] != '/')) return a;

                char op = s[p++];
                Result b = Unary();
                a = Combine(a, b, op);
            }
        }

        private static Result Combine(Result _a, Result _b, char _op)
        {
            if (Failed(_a, _b, out Result worse)) return worse;
            if (!_a.isNumber || !_b.isNumber)
                return Result.Broken($"\"{(_a.isNumber ? _b.text : _a.text)}\" isn't a number, so it can't be used with {_op}.");

            switch (_op)
            {
                case '+': return Result.Number(_a.number + _b.number);
                case '-': return Result.Number(_a.number - _b.number);
                case '*': return Result.Number(_a.number * _b.number);
                default: return Result.Number(_a.number / _b.number);
            }
        }

        private Result Unary()
        {
            if (Take('-'))
            {
                Result value = Unary();
                if (value.state != TokenState.Ok) return value;
                return value.isNumber ? Result.Number(-value.number) : Result.Broken($"\"{value.text}\" isn't a number.");
            }

            if (Take('+'))
                return Unary();

            return Primary();
        }

        private Result Primary()
        {
            Skip();
            if (p >= s.Length)
                return Result.Broken("A value is missing at the end.");

            char c = s[p];

            if (c == '(')
            {
                p++;
                Result inner = Sum();
                return Take(')') || inner.state == TokenState.Broken ? inner : Result.Broken("A ( has no ) after it.");
            }

            if (char.IsDigit(c) || c == '.')
            {
                int start = p;
                while (p < s.Length && (char.IsDigit(s[p]) || s[p] == '.')) p++;

                return double.TryParse(s.Substring(start, p - start), NumberStyles.Float, CultureInfo.InvariantCulture, out double number)
                    ? Result.Number(number)
                    : Result.Broken($"\"{s.Substring(start, p - start)}\" isn't a number.");
            }

            if (char.IsLetter(c) || c == '_')
            {
                int start = p;
                string name = Identifier();

                Skip();
                if (p < s.Length && s[p] == '(' && IsFunction(name))
                {
                    p++;
                    return Call(name);
                }

                p = start;
                return PathValue();
            }

            p++;
            return Result.Broken($"Didn't expect \"{c}\" there.");
        }

        private string Identifier()
        {
            int start = p;
            while (p < s.Length && (char.IsLetterOrDigit(s[p]) || s[p] == '_')) p++;
            return s.Substring(start, p - start);
        }

        private static bool IsFunction(string _name)
        {
            switch (_name.ToLowerInvariant())
            {
                case "round": case "floor": case "ceil": case "abs": case "min": case "max": return true;
                default: return false;
            }
        }

        private Result Call(string _name)
        {
            List<Result> args = new List<Result>();

            if (!Take(')'))
            {
                do args.Add(Sum());
                while (Take(','));

                if (!Take(')'))
                    return Result.Broken($"{_name}( has no ) after it.");
            }

            Result failure = default;
            bool failed = false;
            foreach (Result arg in args)
            {
                if (arg.state == TokenState.Ok && !arg.isNumber)
                    return Result.Broken($"{_name}() takes numbers, and \"{arg.text}\" isn't one.");
                if (arg.state != TokenState.Ok && (!failed || arg.state == TokenState.Broken))
                {
                    failure = arg;
                    failed = true;
                }
            }
            if (failed) return failure;

            string name = _name.ToLowerInvariant();
            int count = args.Count;

            switch (name)
            {
                case "round":
                    if (count == 1) return Result.Number(Math.Round(args[0].number, MidpointRounding.AwayFromZero));
                    if (count == 2) return Result.Number(Math.Round(args[0].number, (int)Math.Max(0, Math.Min(15, args[1].number)), MidpointRounding.AwayFromZero));
                    return Result.Broken("round() takes a number, and optionally how many decimals to keep.");
                case "floor":
                case "ceil":
                case "abs":
                    if (count != 1) return Result.Broken($"{name}() takes one number.");
                    double x = args[0].number;
                    return Result.Number(name == "floor" ? Math.Floor(x) : name == "ceil" ? Math.Ceiling(x) : Math.Abs(x));
                default: // min, max
                    if (count == 0) return Result.Broken($"{name}() needs at least one number.");
                    double best = args[0].number;
                    for (int i = 1; i < count; i++)
                        best = name == "min" ? Math.Min(best, args[i].number) : Math.Max(best, args[i].number);
                    return Result.Number(best);
            }
        }

        // name ( .name | [index] )*
        private Result PathValue()
        {
            int start = p;
            List<object> segments = new List<object> { Identifier() };

            while (p < s.Length)
            {
                if (s[p] == '.')
                {
                    p++;
                    string name = Identifier();
                    if (name.Length == 0) return Result.Broken($"A name is missing after the . in \"{s.Substring(start, p - start)}\".");
                    segments.Add(name);
                }
                else if (s[p] == '[')
                {
                    p++;
                    int digits = p;
                    while (p < s.Length && char.IsDigit(s[p])) p++;
                    if (p == digits || p >= s.Length || s[p] != ']')
                        return Result.Broken("[ ] needs a whole number inside, like [0].");
                    segments.Add(int.Parse(s.Substring(digits, p - digits), CultureInfo.InvariantCulture));
                    p++;
                }
                else break;
            }

            return Resolve(segments);
        }

        private Result Resolve(List<object> _segments)
        {
            string first = (string)_segments[0];
            int next = 1;
            object current;
            string where = first;

            if (TryMember(root, first, out current))
            {
            }
            else if (TryEffect(first, _segments, ref next, out current, out string error))
            {
                if (error != null) return Result.Broken(error);
            }
            else if (string.Equals(first, "Weapon", StringComparison.OrdinalIgnoreCase))
            {
                ItemData_Equipment weapon = Weapon;
                if (weapon == null)
                    return Result.NeedsGame(Application.isPlaying
                        ? "No weapon is worn, so Weapon values show as ?."
                        : "Weapon values come from the worn weapon in game. Pick a Preview Weapon to see them here.");

                // A tooltip number when there is one: it counts the weapon's installed mods, as the tooltip does.
                if (_segments.Count == 2 && _segments[1] is string fieldName &&
                    Enum.TryParse(fieldName, true, out TooltipField field) && !TooltipFields.IsText(field) &&
                    weapon.TryGetTooltipNumber(field, out float value))
                    return Result.Number(value);

                current = weapon;
            }
            else if (string.Equals(first, "Player", StringComparison.OrdinalIgnoreCase))
            {
                PlayerStats stats = Player;
                if (stats == null)
                    return Result.NeedsGame(Application.isPlaying
                        ? "There's no player, so Player values show as ?."
                        : "Player values only exist in game, so they can't be previewed here.");

                current = stats;
            }
            else
            {
                string kind = root != null ? root.GetType().Name : "nothing";
                return Result.Broken($"\"{first}\" isn't a field of this {kind}, or one of its effects.");
            }

            for (; next < _segments.Count; next++)
            {
                object segment = _segments[next];

                if (current == null || (current is UnityEngine.Object asset && asset == null))
                    return Result.Broken($"{where} is empty, so it has no {segment}.");

                if (segment is int index)
                {
                    if (!(current is IList list))
                        return Result.Broken($"{where} isn't a list, so [{index}] doesn't work on it.");
                    if (index >= list.Count)
                        return Result.Broken($"{where} has {list.Count} entries, so [{index}] is past the end.");

                    current = list[index];
                    where += $"[{index}]";
                }
                else
                {
                    string name = (string)segment;
                    object owner = current;
                    if (!TryMember(owner, name, out current))
                        return Result.Broken($"{owner.GetType().Name} has no field \"{name}\".");

                    where += "." + name;
                }
            }

            return FromValue(current);
        }

        // An effect in one of the root's lists, by type name; a following [i] picks among several of that type.
        private bool TryEffect(string _name, List<object> _segments, ref int _next, out object _effect, out string _error)
        {
            _effect = null;
            _error = null;

            List<object> matches = new List<object>();
            foreach (object listed in ListedObjects(root))
                if (IsNamed(listed.GetType(), _name))
                    matches.Add(listed);

            if (matches.Count == 0) return false;

            int wanted = 0;
            if (_next < _segments.Count && _segments[_next] is int index)
            {
                wanted = index;
                _next++;
            }

            if (wanted >= matches.Count)
                _error = $"There {(matches.Count == 1 ? "is 1" : $"are {matches.Count}")} {_name}, so [{wanted}] is past the end.";
            else
                _effect = matches[wanted];

            return true;
        }
    }
}
