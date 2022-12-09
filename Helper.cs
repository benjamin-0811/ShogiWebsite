using System.Text;

namespace ShogiWebsite
{
    internal class Helper
    {
        internal static string RepeatString(string sequence, int amount) => new StringBuilder().Insert(0, sequence, amount).ToString();

        internal static void AddRepeatChar(StringBuilder builder, char character, int amount)
        {
            for (int i = 0; i < amount; i++)
                builder.Append(character);
        }

        internal static string Indent(int amount, string message = "") => new StringBuilder(RepeatString("\t", amount)).Append(message).ToString();

        internal static void AddIndent(StringBuilder builder, int amount) => AddRepeatChar(builder, '\t', amount);

        internal static bool LastIsNewLine(StringBuilder builder) => builder.Length > 0 && builder[^1] == '\n';
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
        private readonly Dictionary<string, string?> properties;
        private readonly List<object> children;
        private int depth;

        private static readonly Dictionary<string, TagType> tagTypeLookUpTable = CreateTagTypeLookUpTable();

        internal HtmlBuilder(string tag, TagType tagType)
        {
            this.tag = tag;
            this.tagType = tagType;
            properties = new();
            children = new();
            depth = 0;
        }

        internal HtmlBuilder(string tag = "div") : this(tag, FindTagType(tag))
        { }

        [Flags]
        internal enum TagType
        {
            /// <summary>no closing tag</summary>
            SingleInline = 0b_0000_0001,
            /// <summary>no closing tag, requires own line</summary>
            SingleOwnLine = 0b_0000_0010,
            /// <summary>in the same line as plain text</summary>
            BlockInline = 0b_0000_0100,
            /// <summary>opening and closing tag with content in the same line</summary>
            BlockOneLine = 0b_0000_1000,
            /// <summary>opening and closing tag with content in their own line, one tab indent</summary>
            Block = 0b_0001_0000,
            /// <summary>see block, no indentation</summary>
            BlockNoIndent = 0b_0010_0000,
            /// <summary>preformatted text, content starts with no indentation whatsoever</summary>
            Preformatted = 0b_0100_0000,
            Unpaired = SingleInline | SingleOwnLine,
            Paired = BlockInline | BlockOneLine | Block | BlockNoIndent | Preformatted,
            NewLine = SingleOwnLine | BlockOneLine | Block | BlockNoIndent | Preformatted,
            ChildNewLine = Block | BlockNoIndent | Preformatted
        }

        private static Dictionary<string, TagType> CreateTagTypeLookUpTable()
        {
            Dictionary<string, TagType> dict = new();
            // Add more if there are any missing html tags.
            dict["!DOCTYPE"] = TagType.SingleOwnLine;
            dict["a"] = TagType.BlockInline;
            dict["abbr"] = TagType.BlockInline;
            dict["address"] = TagType.Block;
            dict["area"] = TagType.SingleInline;
            dict["article"] = TagType.Block;
            dict["aside"] = TagType.Block;
            dict["audio"] = TagType.Block;
            dict["b"] = TagType.BlockInline;
            dict["base"] = TagType.SingleOwnLine;
            dict["bdi"] = TagType.BlockInline;
            dict["bdo"] = TagType.BlockInline;
            dict["blockquote"] = TagType.Block;
            dict["body"] = TagType.Block;
            dict["br"] = TagType.SingleInline;
            dict["button"] = TagType.BlockInline;
            dict["canvas"] = TagType.Block;
            dict["caption"] = TagType.BlockOneLine;
            dict["cite"] = TagType.BlockInline;
            dict["code"] = TagType.BlockInline;
            dict["col"] = TagType.SingleOwnLine;
            dict["colgroup"] = TagType.Block;
            dict["data"] = TagType.BlockInline;
            dict["datalist"] = TagType.Block;
            dict["dd"] = TagType.BlockOneLine;
            dict["del"] = TagType.BlockInline;
            dict["details"] = TagType.Block;
            dict["dfn"] = TagType.BlockInline;
            dict["dialog"] = TagType.BlockOneLine;
            dict["div"] = TagType.Block;
            dict["dl"] = TagType.Block;
            dict["dt"] = TagType.BlockOneLine;
            dict["em"] = TagType.BlockInline;
            dict["embed"] = TagType.SingleOwnLine;
            dict["fieldset"] = TagType.Block;
            dict["figcaption"] = TagType.BlockOneLine;
            dict["figure"] = TagType.Block;
            dict["footer"] = TagType.Block;
            dict["form"] = TagType.Block;
            dict["h1"] = TagType.BlockOneLine;
            dict["h2"] = TagType.BlockOneLine;
            dict["h3"] = TagType.BlockOneLine;
            dict["h4"] = TagType.BlockOneLine;
            dict["h5"] = TagType.BlockOneLine;
            dict["h6"] = TagType.BlockOneLine;
            dict["head"] = TagType.Block;
            dict["header"] = TagType.Block;
            dict["hr"] = TagType.SingleOwnLine;
            dict["html"] = TagType.BlockNoIndent;
            dict["i"] = TagType.BlockInline;
            dict["iframe"] = TagType.Block;
            dict["img"] = TagType.SingleOwnLine;
            dict["input"] = TagType.SingleOwnLine;
            dict["ins"] = TagType.BlockInline;
            dict["kbd"] = TagType.BlockInline;
            dict["label"] = TagType.BlockOneLine;
            dict["legend"] = TagType.BlockOneLine;
            dict["li"] = TagType.BlockOneLine;
            dict["link"] = TagType.SingleOwnLine;
            dict["main"] = TagType.Block;
            dict["map"] = TagType.Block;
            dict["mark"] = TagType.BlockInline;
            dict["meta"] = TagType.SingleOwnLine;
            dict["meter"] = TagType.BlockOneLine;
            dict["nav"] = TagType.Block;
            dict["noscript"] = TagType.BlockOneLine;
            dict["object"] = TagType.Block;
            dict["ol"] = TagType.Block;
            dict["optgroup"] = TagType.Block;
            dict["option"] = TagType.BlockOneLine;
            dict["output"] = TagType.BlockOneLine;
            dict["p"] = TagType.BlockOneLine;
            dict["param"] = TagType.SingleOwnLine;
            dict["picture"] = TagType.Block;
            dict["pre"] = TagType.Preformatted;
            dict["progress"] = TagType.BlockOneLine;
            dict["q"] = TagType.BlockInline;
            dict["rp"] = TagType.BlockInline;
            dict["rt"] = TagType.BlockInline;
            dict["ruby"] = TagType.Block;
            dict["s"] = TagType.BlockInline;
            dict["samp"] = TagType.BlockInline;
            dict["script"] = TagType.Block;
            dict["section"] = TagType.Block;
            dict["select"] = TagType.Block;
            dict["small"] = TagType.BlockInline;
            dict["source"] = TagType.SingleOwnLine;
            dict["span"] = TagType.BlockInline;
            dict["strong"] = TagType.BlockInline;
            dict["style"] = TagType.Block;
            dict["sub"] = TagType.BlockInline;
            dict["summary"] = TagType.BlockOneLine;
            dict["sup"] = TagType.BlockInline;
            dict["svg"] = TagType.Block;
            dict["table"] = TagType.Block;
            dict["tbody"] = TagType.Block;
            dict["td"] = TagType.BlockOneLine;
            dict["template"] = TagType.Block;
            dict["textarea"] = TagType.Preformatted;
            dict["tfoot"] = TagType.Block;
            dict["th"] = TagType.BlockOneLine;
            dict["thead"] = TagType.Block;
            dict["time"] = TagType.BlockInline;
            dict["title"] = TagType.BlockOneLine;
            dict["tr"] = TagType.Block;
            dict["track"] = TagType.SingleOwnLine;
            dict["u"] = TagType.BlockInline;
            dict["ul"] = TagType.Block;
            dict["var"] = TagType.BlockInline;
            dict["video"] = TagType.Block;
            dict["wbr"] = TagType.SingleInline;
            return dict;
        }

        private static TagType FindTagType(string tag)
        {
            try
            {
                return tagTypeLookUpTable[tag];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return TagType.Block;
            }
        }

        internal HtmlBuilder Property(string key, string? value = null)
        {
            properties[key] = value;
            return this;
        }

        internal HtmlBuilder Child(object child)
        {
            if (child is HtmlBuilder builder)
                builder.depth = depth + 1;
            children.Add(child);
            return this;
        }

        // Use StringBuilder from Parent
        internal override string Build()
        {
            StringBuilder builder = new();
            FillBuilder(builder);
            return builder.ToString();
        }

        private void FillBuilder(StringBuilder builder)
        {
            if (tagType == (tagType & TagType.Paired))
                AddPairedTag(builder);
            else if (tagType == (tagType & TagType.Unpaired))
                AddUnpairedTag(builder);
        }

        private void AddStartTag(StringBuilder builder)
        {
            builder.Append('<').Append(tag);
            AddProperties(builder);
            builder.Append('>');
        }

        private void AddEndTag(StringBuilder builder)
        {
            builder.Append($"</{tag}>");
        }

        private void AddSingleTag(StringBuilder builder)
        {
            builder.Append('<').Append(tag);
            AddProperties(builder);
            builder.Append("/>");
        }

        private void AddPairedTag(StringBuilder builder)
        {
            bool newLine = tagType == (tagType & TagType.NewLine);
            bool childNewLine = tagType == (tagType & TagType.ChildNewLine);
            if (newLine)
                NextLine(builder);
            AddStartTag(builder);
            if (childNewLine)
                builder.Append('\n');
            foreach (object child in children)
                AddChild(builder, child);
            if (childNewLine)
                NextLine(builder);
            AddEndTag(builder);
            if (newLine)
                builder.Append('\n');
        }

        private void AddUnpairedTag(StringBuilder builder)
        {
            bool newLine = tagType == (tagType & TagType.NewLine);
            if (newLine)
                NextLine(builder);
            AddSingleTag(builder);
            if (newLine)
                builder.Append('\n');
        }

        private void NextLine(StringBuilder builder)
        {
            if (!Helper.LastIsNewLine(builder))
                builder.Append('\n');
            Helper.AddIndent(builder, depth);
        }

        private void AddProperties(StringBuilder builder)
        {
            foreach (var property in properties)
            {
                if (property.Value == null)
                    builder.Append($" {property.Key}");
                else
                    builder.Append($" {property.Key}=\"{property.Value}\"");
            }
        }

        private void AddChild(StringBuilder builder, object child)
        {
            if (child is string stringChild)
            {
                string[] lines = stringChild.Split('\n');
                if (lines.Length > 1)
                {
                    foreach (string line in lines)
                    {
                        AddChild(builder, line);
                        builder.Append('\n');
                    }
                    return;
                }
            }
            if (child is decimal or string || child.GetType().IsPrimitive)
            {
                if (Helper.LastIsNewLine(builder))
                {
                    if (tagType == TagType.Block)
                        Helper.AddIndent(builder, depth + 1);
                    else if (tagType == TagType.BlockNoIndent)
                        Helper.AddIndent(builder, depth);
                }
                builder.Append(child);
            }
            else if (child is HtmlBuilder childBuilder)
                childBuilder.FillBuilder(builder);
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
