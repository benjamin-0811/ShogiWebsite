using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace ShogiWebsite;

internal struct Pair<TX, TY>
{
    private TX x;
    private TY y;

    internal TX X { get => this.x; set => this.x = value; }
    internal TY Y { get => this.y; set => this.y = value; }

    internal Pair(TX x, TY y)
    {
        this.x = x;
        this.y = y;
    }

}

internal class Helper
{
    internal static void RepeatChar(StringBuilder builder, char character, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            builder.Append(character);
        }
    }

    internal static void Indent(StringBuilder builder, int amount)
    {
        RepeatChar(builder, '\t', amount);
    }

    internal static bool LastIsNewLine(StringBuilder builder)
    {
        return builder.Length > 0 && builder[^1] == '\n';
    }

    internal static void AssertNotNull([NotNull] object? obj)
    {
        if (obj == null)
        {
            throw new NullReferenceException("Given object was null, even though it shouldn't have been.");
        }
    }
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

internal class CssBuilder : AbstractBuilder<string>
{
    private readonly List<string> selectors;
    private readonly Dictionary<string, string> styles;
    private readonly int indentTabs;

    internal CssBuilder(int indentTabs = 1)
    {
        this.selectors = new();
        this.styles = new();
        this.indentTabs = indentTabs;
    }

    internal CssBuilder Selector(string selector)
    {
        if (!this.selectors.Contains(selector))
        {
            this.selectors.Add(selector);
        }
        return this;
    }

    internal CssBuilder Style(string key, string value)
    {
        this.styles[key] = value;
        return this;
    }

    internal CssBuilder DeleteSytle(string key)
    {
        this.styles.Remove(key);
        return this;
    }

    internal override string Build()
    {
        bool noSelectors = this.selectors.Count <= 0;
        bool noStyles = this.styles.Count <= 0;
        if (noSelectors || noStyles)
        {
            return "";
        }
        StringBuilder builder = new();
        this.AddSelectors(builder);
        builder.Append(' ');
        this.AddStyles(builder);
        // selectors.Clear();
        // styles.Clear();
        return builder.ToString();
    }

    private void AddSelectors(StringBuilder builder)
    {
        int length = this.selectors.Count;
        if (length <= 0)
        {
            return;
        }
        Helper.Indent(builder, this.indentTabs);
        for (int i = 0; i < length - 1; i++)
        {
            builder.Append($"{this.selectors[i]}, ");
        }
        builder.Append(this.selectors[length - 1]);
    }

    private void AddStyle(StringBuilder builder, KeyValuePair<string, string> style)
    {
        Helper.Indent(builder, this.indentTabs + 1);
        builder.AppendLine($"{style.Key}: {style.Value};");
    }

    private void AddStyles(StringBuilder builder)
    {
        builder.Append('{');
        int length = this.styles.Count;
        if (length <= 0)
        {
            builder.AppendLine(" }");
            return;
        }
        builder.Append('\n');
        foreach (KeyValuePair<string, string> style in this.styles)
        {
            this.AddStyle(builder, style);
        }
        Helper.Indent(builder, this.indentTabs);
        builder.AppendLine("}");
    }

    internal override CssBuilder Reset()
    {
        this.selectors.Clear();
        this.styles.Clear();
        return this;
    }
}

internal class LinesBuilder : AbstractBuilder<string>
{
    private readonly List<Pair<string, int>> lines;
    private readonly int indentTabs;

    internal LinesBuilder(int indentTabs = 1)
    {
        this.lines = new List<Pair<string, int>>();
        this.indentTabs = indentTabs;
    }

    internal LinesBuilder Line(string line, int additionalIndent = 0)
    {
        this.lines.Add(new Pair<string, int>(line, additionalIndent));
        return this;
    }

    internal LinesBuilder EmptyLine()
    {
        this.lines.Add(new Pair<string, int>("", 0));
        return this;
    }

    internal LinesBuilder RemoveLine(int index)
    {
        if (index < this.lines.Count)
        {
            this.lines.RemoveAt(index);
        }
        return this;
    }

    internal LinesBuilder RemoveLastLine()
    {
        return this.RemoveLine(this.lines.Count - 1);
    }

    internal override string Build()
    {
        int length = this.lines.Count;
        if (length <= 0)
        {
            return "";
        }
        StringBuilder builder = new();
        for (int i = 0; i < length; i++)
        {
            this.AddLine(builder, i);
        }
        // lines.Clear();
        return builder.ToString();
    }

    private void AddLine(StringBuilder builder, int index)
    {
        if (index >= this.lines.Count)
        {
            return;
        }
        Pair<string, int> line = this.lines[index];
        Helper.Indent(builder, this.indentTabs + line.Y);
        builder.AppendLine(line.X);
    }

    internal override LinesBuilder Reset()
    {
        this.lines.Clear();
        return this;
    }
}

internal class HtmlBuilder : AbstractBuilder<string>
{
    private readonly string tag;
    private readonly TagType tagType;
    private readonly Dictionary<string, string?> properties;
    private readonly List<object> children;
    private int depth;

    private static readonly Dictionary<string, TagType> tagTypeLookUpTable = ReadTagTypeLookUpTable();

    internal HtmlBuilder(string tag, TagType tagType)
    {
        this.tag = tag;
        this.tagType = tagType;
        this.properties = new();
        this.children = new();
        this.depth = 0;
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

    internal static TagType StringToTagType(string tagName) => tagName switch
    {
        "singleInline" => TagType.SingleInline,
        "singleOwnLine" => TagType.SingleOwnLine,
        "blockInline" => TagType.BlockInline,
        "blockOneLine" => TagType.BlockOneLine,
        "block" => TagType.Block,
        "blockNoIndent" => TagType.BlockNoIndent,
        "preformatted" => TagType.Preformatted,
        _ => TagType.Block
    };

    private static Dictionary<string, TagType> ReadTagTypeLookUpTable()
    {
        Dictionary<string, TagType> dict = new();
        string tagsJson = File.ReadAllText(Program.projectDir + @"\assets\json\html_tags.json");
        Dictionary<string, string>? jsonObject = JsonSerializer.Deserialize<Dictionary<string, string>>(tagsJson);
        if (jsonObject == null)
        {
            return dict;
        }
        foreach (KeyValuePair<string, string> jsonProperty in jsonObject)
        {
            dict[jsonProperty.Key] = StringToTagType(jsonProperty.Value);
        }
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
        this.properties[key] = value;
        return this;
    }

    internal HtmlBuilder Id(string value)
    {
        return this.Property("id", value);
    }

    internal HtmlBuilder Class(string value)
    {
        return this.Property("class", value);
    }

    internal HtmlBuilder Style(string value)
    {
        return this.Property("style", value);
    }

    internal HtmlBuilder Child(object child)
    {
        if (child is HtmlBuilder builder)
        {
            builder.depth = this.depth + 1;
        }
        this.children.Add(child);
        return this;
    }

    internal override string Build()
    {
        StringBuilder builder = new("<!DOCTYPE html>");
        this.FillBuilder(builder);
        this.properties.Clear();
        // children.Clear();
        return builder.ToString();
    }

    private void FillBuilder(StringBuilder builder)
    {
        if (this.tagType == (this.tagType & TagType.Paired))
        {
            this.AddPairedTag(builder);
        }
        else if (this.tagType == (this.tagType & TagType.Unpaired))
        {
            this.AddUnpairedTag(builder);
        }
    }

    private void AddStartTag(StringBuilder builder)
    {
        builder.Append('<').Append(this.tag);
        this.AddProperties(builder);
        builder.Append('>');
    }

    private void AddEndTag(StringBuilder builder)
    {
        builder.Append($"</{this.tag}>");
    }

    private void AddSingleTag(StringBuilder builder)
    {
        builder.Append('<').Append(this.tag);
        this.AddProperties(builder);
        builder.Append("/>");
    }

    private void AddPairedTag(StringBuilder builder)
    {
        if (!(this.children.Any() || this.properties.Any()))
        {
            return;
        }
        bool newLine = this.tagType == (this.tagType & TagType.NewLine);
        bool childNewLine = this.tagType == (this.tagType & TagType.ChildNewLine);
        if (newLine)
        {
            this.NextLine(builder);
        }
        this.AddStartTag(builder);
        if (childNewLine)
        {
            builder.Append('\n');
        }
        foreach (object child in this.children)
        {
            this.AddChild(builder, child);
        }
        if (childNewLine)
        {
            this.NextLine(builder);
        }
        this.AddEndTag(builder);
        if (newLine)
        {
            builder.Append('\n');
        }
    }

    private void AddUnpairedTag(StringBuilder builder)
    {
        bool newLine = this.tagType == (this.tagType & TagType.NewLine);
        if (newLine)
        {
            this.NextLine(builder);
        }
        this.AddSingleTag(builder);
        if (newLine)
        {
            builder.Append('\n');
        }
    }

    private void NextLine(StringBuilder builder)
    {
        if (!Helper.LastIsNewLine(builder))
        {
            builder.Append('\n');
        }
        Helper.Indent(builder, this.depth);
    }

    private void AddProperties(StringBuilder builder)
    {
        foreach (KeyValuePair<string, string?> property in this.properties)
        {
            if (property.Value == null)
            {
                builder.Append($" {property.Key}");
            }
            else
            {
                builder.Append($" {property.Key}=\"{property.Value}\"");
            }
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
                    this.AddChild(builder, line);
                    builder.Append('\n');
                }
                return;
            }
        }
        if (child is decimal or string || child.GetType().IsPrimitive)
        {
            if (Helper.LastIsNewLine(builder))
            {
                if (this.tagType == TagType.Block)
                {
                    Helper.Indent(builder, this.depth + 1);
                }
                else if (this.tagType == TagType.BlockNoIndent)
                {
                    Helper.Indent(builder, this.depth);
                }
            }
            builder.Append(child);
        }
        else if (child is HtmlBuilder childBuilder)
        {
            childBuilder.depth = this.depth + 1;
            childBuilder.FillBuilder(builder);
        }
    }

    internal override HtmlBuilder Reset()
    {
        this.properties.Clear();
        this.children.Clear();
        return this;
    }
}

internal class Color
{
    private byte r, g, b, a;
    private int rgba;

    internal readonly static Color RED = new(255, 0, 0);

    internal byte R
    {
        get => this.r;
        set { this.r = value; this.CalcRgba(); }
    }

    internal byte G
    {
        get => this.g;
        set { this.g = value; this.CalcRgba(); }
    }

    internal byte B
    {
        get => this.b;
        set { this.b = value; this.CalcRgba(); }
    }

    internal byte A
    {
        get => this.a;
        set { this.a = value; this.CalcRgba(); }
    }

    internal int Rgba
    {
        get => this.rgba;
        set { this.rgba = value; this.CalcRgbaBytes(); }
    }

    internal Color(byte r, byte g, byte b, byte a = 255)
    {
        this.r = r;
        this.g = g;
        this.b = b;
        this.a = a;
        this.CalcRgba();
    }

    internal Color(int rgba)
    {
        this.rgba = rgba;
        this.CalcRgbaBytes();
    }

    private void CalcRgba()
    {
        this.rgba = BitConverter.ToInt32(new byte[] { this.a, this.b, this.g, this.r });
    }

    private void CalcRgbaBytes()
    {
        byte[] brgab = BitConverter.GetBytes(this.rgba);
        this.r = brgab[0];
        this.g = brgab[1];
        this.b = brgab[2];
        this.a = brgab[3];
    }

    internal string RgbaHex()
    {
        return "#" + this.rgba.ToString("X");
    }

    internal string RgbHex()
    {
        return this.RgbaHex()[..7];
    }
}