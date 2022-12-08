using System.Text;

namespace ShogiWebsite
{
    internal class Helper
    {
        internal static string RepeatString(string sequence, int amount) => new StringBuilder().Insert(0, sequence, amount).ToString();

        internal static string Indent(int amount, string message = "") => new StringBuilder(RepeatString("\t", amount)).Append(message).ToString();
    }

    internal class BetterConsole
    {
        internal static bool printAction = false;
        internal static bool printError = false;
        internal static bool printInfo = false;
        internal static bool printSpecial = false;
        internal static ConsoleColor actionColor = ConsoleColor.Yellow;
        internal static ConsoleColor errorColor = ConsoleColor.Red;
        internal static ConsoleColor infoColor = ConsoleColor.Green;
        internal static ConsoleColor specialColor = ConsoleColor.Cyan;

        internal static void Action(string message)
        {
            if (printAction)
            {
                Console.ForegroundColor = actionColor;
                Console.WriteLine($"Action  : {message}");
                Console.ResetColor();
            }
        }

        internal static void Error(string message)
        {
            if (printError)
            {
                Console.ForegroundColor = errorColor;
                Console.WriteLine($"Error   : {message}");
                Console.ResetColor();
            }
        }

        internal static void Info(string message)
        {
            if (printInfo)
            {
                Console.ForegroundColor = infoColor;
                Console.WriteLine($"Info    : {message}");
                Console.ResetColor();
            }
        }

        internal static void Special(string message)
        {
            if (printSpecial)
            {
                Console.ForegroundColor = specialColor;
                Console.WriteLine($"Special : {message}");
                Console.ResetColor();
            }
        }
    }

    internal abstract class AbstractBuilder<T>
    {
        internal abstract T Build();
        internal abstract AbstractBuilder<T> Reset();
    }

    /// <summary>easier way to dynamically write css styles</summary>
    internal class CssBuilder : AbstractBuilder<string>
    {
        private readonly List<string> selectors;
        private readonly List<KeyValuePair<string, string>> styles;
        private readonly int indentTabs;

        internal CssBuilder(int indentTabs = 1)
        {
            selectors = new();
            styles = new();
            this.indentTabs = indentTabs;
        }

        internal CssBuilder Selector(string selector)
        {
            if (!selectors.Contains(selector))
                selectors.Add(selector);
            return this;
        }

        internal CssBuilder Style(string key, string value)
        {
            // Find out if the key already exists, if it does replace it
            KeyValuePair<string, string>? style = null;
            foreach (KeyValuePair<string, string> s in styles)
            {
                if (s.Key == key)
                {
                    style = s;
                    break;
                }
            }
            if (style != null)
                styles.Remove((KeyValuePair<string, string>)style);
            styles.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        internal CssBuilder DeleteSytle(string key)
        {
            foreach(var s in styles)
            {
                if (s.Key == key)
                {
                    styles.Remove(s);
                    break;
                }
            }
            return this;
        }

        internal override string Build() => $"{SelectorsToString()} {StylesToString()}";

        private string SelectorsToString()
        {
            int length = selectors.Count;
            if (length <= 0)
                return "";
            StringBuilder builder = new(Helper.Indent(indentTabs));
            for (int i = 0; i < length - 1; i++)
                builder.Append($"{selectors[i]}, ");
            return builder.Append($"{selectors[length - 1]}").ToString(); ;
        }

        private string StyleToString(int index)
        {
            int length = styles.Count;
            if (index >= length)
                return "";
            KeyValuePair<string, string> s = styles[index];
            return $"{s.Key}: {s.Value};";
        }

        private string StylesToString()
        {
            int length = styles.Count;
            if (length <= 0)
                return "{ }";
            StringBuilder builder = new("{");
            for (int i = 0; i < length; i++)
                builder.Append(Helper.RepeatString("  ", indentTabs + 2) + StyleToString(i));
            return builder.Append(Helper.RepeatString("  ", indentTabs) + "}").ToString();
        }

        internal override CssBuilder Reset() => new(indentTabs);
    }

    /// <summary>easier way to dynamically write a text on multiple lines</summary>
    internal class LinesBuilder : AbstractBuilder<string>
    {
        // lines with additional amount of 4-space tabs
        private readonly List<KeyValuePair<string, int>> lines;
        // amount of double spaces as ground space
        private readonly int indentTabs;

        internal LinesBuilder(int indentTabs = 1)
        {
            lines = new List<KeyValuePair<string, int>>();
            this.indentTabs = indentTabs;
        }

        internal LinesBuilder Line(string line, int additionalIndent = 0)
        {
            lines.Add(new KeyValuePair<string, int>(line, additionalIndent));
            return this;
        }

        internal LinesBuilder EmptyLine()
        {
            lines.Add(new KeyValuePair<string, int>("", 0));
            return this;
        }

        internal LinesBuilder RemoveLine(int index)
        {
            if (index < lines.Count)
                lines.RemoveAt(index);
            return this;
        }

        internal LinesBuilder RemoveLastLine() => RemoveLine(lines.Count - 1);

        internal override string Build() => Build(false);

        internal string Build(bool has4SpaceTab)
        {
            int length = lines.Count;
            if (length <= 0)
                return "";
            StringBuilder builder = new(WriteLine(0, has4SpaceTab));
            for (int i = 1; i < length; i++)
                builder.AppendLine(WriteLine(i, has4SpaceTab));
            return builder.ToString();
        }

        private string WriteLine(int index, bool has4SpaceTab = true)
        {
            if (index >= lines.Count)
                return "";
            KeyValuePair<string, int> line = lines[index];
            return Helper.Indent(indentTabs + line.Value, line.Key);
        }

        internal override LinesBuilder Reset() => new(indentTabs);
    }

    internal class HtmlBuilder : AbstractBuilder<string>
    {
        private readonly string tag;
        private readonly TagType tagType;
        private readonly Dictionary<string, string> properties;
        private readonly List<object> children;
        private int depth;
        private static readonly Dictionary<string, TagType> tagTypeLookUpTable = CreateTagTypeLookUpTable();
        internal HtmlBuilder(string tag = "div")
        {
            this.tag = tag;
            tagType = FindTagType();
            properties = new();
            children = new();
            depth = 0;
        }

        private enum TagType
        {
            Single,
            Inline,
            Block,
            BlockNoIndent
        }

        private static Dictionary<string, TagType> CreateTagTypeLookUpTable()
        {
            Dictionary<string, TagType> dict = new();
            dict["!DOCTYPE"] = TagType.Single;
            dict["a"] = TagType.Inline;
            dict["abbr"] = TagType.Inline;
            dict[""] =

            return dict;
        }

        private TagType FindTagType()
        {
            bool isBlockTag = blockTags.Contains(tag);
            bool isSelfClosing = selfClodingTags.Contains(tag);
            if (isBlockTag)
                return TagType.UsualBlock;
            if (isSelfClosing)
                return TagType.SelfClosing;

        }

        internal HtmlBuilder Property(string key, string value)
        {
            properties[key] = value;
            return this;
        }

        internal HtmlBuilder Child(object child)
        {
            if (!blockTags.Contains(tag))
                return this;
            if (child is HtmlBuilder builder)
                builder.depth = depth + 1;
            children.Add(child);
            return this;
        }

        // Use StringBuilder from Parent
        internal override string Build() => FillBuilder(new StringBuilder()).ToString();

        private StringBuilder FillBuilder(StringBuilder builder)
        {
            builder = builder.AppendLine($"<{tag} {PropertiesToString()}>");
            if (!blockTags.Contains(tag))
                return builder;
            foreach (var child in children)
                builder = builder.AppendLine(ChildToString(child));
            return builder.AppendLine(Helper.Indent(depth, $"</{tag}>"));
        }

        private string PropertiesToString()
        {
            StringBuilder builder = new();
            foreach (var property in properties)
                builder.Append($"{property.Key}=\"{property.Value}\" ");
            return builder.ToString().Trim();
        }

        private string ChildToString(object child)
        {
            StringBuilder builder = new(Helper.RepeatString("\t", depth + 1));
            if (child is decimal decimalChild)
                child = decimalChild.ToString();
            if (child is string || child.GetType().IsPrimitive)
                return builder.Append(child).ToString();
            else if (child is HtmlBuilder childBuilder)
                return builder.Append(childBuilder.Build()).ToString();
            else
                return "";
        }

        internal override HtmlBuilder Reset() => new(tag);
    }

    internal class Color
    {
        private byte r, g, b, a;
        private int rgba;

        internal readonly static Color RED = new(255, 0, 0);

        internal byte R
        {
            get => r;
            set { r = value; CalcRgba(); }
        }

        internal byte G
        {
            get => g;
            set { g = value; CalcRgba(); }
        }

        internal byte B
        {
            get => b;
            set { b = value; CalcRgba(); }
        }

        internal byte A
        {
            get => a;
            set { a = value; CalcRgba(); }
        }

        internal int Rgba
        {
            get => rgba;
            set { rgba = value; CalcRgbaBytes(); }
        }

        internal Color(byte r, byte g, byte b, byte a = 255)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
            CalcRgba();
        }

        internal Color(int rgba)
        {
            this.rgba = rgba;
            CalcRgbaBytes();
        }

        private void CalcRgba()
        {
            rgba = BitConverter.ToInt32(new byte[] { a, b, g, r });
        }

        private void CalcRgbaBytes()
        {
            byte[] brgab = BitConverter.GetBytes(rgba);
            r = brgab[0];
            g = brgab[1];
            b = brgab[2];
            a = brgab[3];
        }

        internal string RgbaHex() => "#" + rgba.ToString("X");

        internal string RgbHex() => RgbaHex()[..7];
    }
}
