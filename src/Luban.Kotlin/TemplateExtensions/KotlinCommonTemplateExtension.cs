using Luban;
using Luban.Kotlin.TypeVisitors;
using Luban.Defs;
using Luban.Types;
using Luban.Utils;
using Scriban.Runtime;
using System.Text;

namespace Luban.Kotlin.TemplateExtensions;

public class KotlinCommonTemplateExtension : ScriptObject
{
    private const string AsteriaAnnotationPrefix = "io.github.realmlabs.asteria.config.annotations";

    public static string DeclaringTypeName(TType type)
    {
        return type.Apply(KotlinDeclaringTypeNameVisitor.Ins);
    }

    public static string DeclaringBoxTypeName(TType type)
    {
        return type.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins);
    }

    public static string PascalName(string name)
    {
        return TypeUtil.ToPascalCase(name);
    }

    public static string KotlinAnnotations(object obj)
    {
        var tags = obj switch
        {
            DefTypeBase defType => defType.Tags,
            DefField field => field.Tags,
            _ => null,
        };
        if (tags == null || tags.Count == 0)
        {
            return "";
        }

        var annotations = new List<string>();
        AddAnnotations(tags, "kotlin.annotation", annotations);
        AddAnnotations(tags, "kotlin.annotations", annotations);
        return string.Join('\n', annotations);
    }

    public static string Indent(string text, string prefix)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }
        return prefix + text.Replace("\n", "\n" + prefix);
    }

    public static string AsteriaConfigCatalogAnnotation()
    {
        if (!IsAsteriaConfigEnabled() || !GetBoolOption("kotlin.asteria.config.catalog", true))
        {
            return "";
        }

        var args = new List<string>();
        AddStringArg(args, "packageName", GetOption("kotlin.asteria.config.packageName", ""));
        AddStringArg(args, "tablesObjectName", GetOption("kotlin.asteria.config.tablesObjectName", ""));
        AddStringArg(args, "accessorClassName", GetOption("kotlin.asteria.config.accessorClassName", ""));

        return BuildAnnotation($"{AsteriaAnnotationPrefix}.AsteriaConfigCatalog", args);
    }

    public static string AsteriaConfigTableAnnotation(DefTable table)
    {
        if (!ShouldEmitAsteriaConfigTable(table))
        {
            return "";
        }

        string shape = table.IsMapTable ? "KEYED" : table.IsListTable ? "LIST" : "SINGLETON";
        shape = GetTagOrDefault(table, "asteria.shape", shape).ToUpperInvariant();

        var args = new List<string>
        {
            $"name = {KotlinString(GetTagOrDefault(table, "asteria.name", table.OutputDataFile))}",
        };

        if (shape != "KEYED")
        {
            args.Add($"shape = {AsteriaAnnotationPrefix}.AsteriaConfigTableShape.{shape}");
        }

        if (shape == "KEYED")
        {
            string keyType = GetTagOrDefault(table, "asteria.keyType", table.KeyTType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins));
            args.Add($"keyType = {keyType}::class");
        }

        string rowType = GetTagOrDefault(table, "asteria.rowType", table.ValueTType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins));
        args.Add($"rowType = {rowType}::class");

        string tableType = GetTagOrDefault(table, "asteria.tableType", GetOption("kotlin.asteria.config.tableType", ""));
        if (!string.IsNullOrWhiteSpace(tableType))
        {
            args.Add($"tableType = {tableType}::class");
        }

        AddStringArg(args, "refName", GetTagOrDefault(table, "asteria.refName", ""));
        AddStringArg(args, "propertyName", GetTagOrDefault(table, "asteria.propertyName", ""));

        return BuildAnnotation($"{AsteriaAnnotationPrefix}.AsteriaConfigTable", args);
    }

    private static bool ShouldEmitAsteriaConfigTable(DefTable table)
    {
        if (GetBoolTag(table, "asteria.skip", false))
        {
            return false;
        }
        return IsAsteriaConfigEnabled() || GetBoolTag(table, "asteria", false) || GetBoolTag(table, "asteria.config", false);
    }

    private static bool IsAsteriaConfigEnabled()
    {
        return GetBoolOption("kotlin.asteria.config.enabled", false)
               || GetBoolOption("kotlin.asteriaConfigAnnotations", false);
    }

    private static void AddAnnotations(Dictionary<string, string> tags, string key, List<string> output)
    {
        if (!tags.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        foreach (var annotation in value.Split(new[] { '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var text = annotation.Trim();
            if (text.Length == 0)
            {
                continue;
            }
            output.Add(text.StartsWith('@') ? text : $"@{text}");
        }
    }

    private static string BuildAnnotation(string name, List<string> args)
    {
        if (args.Count == 0)
        {
            return $"@{name}";
        }

        var builder = new StringBuilder();
        builder.Append('@').Append(name).AppendLine("(");
        for (int i = 0; i < args.Count; i++)
        {
            builder.Append("    ").Append(args[i]);
            if (i != args.Count - 1)
            {
                builder.Append(',');
            }
            builder.AppendLine();
        }
        builder.Append(')');
        return builder.ToString();
    }

    private static void AddStringArg(List<string> args, string name, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            args.Add($"{name} = {KotlinString(value)}");
        }
    }

    private static string KotlinString(string value)
    {
        var builder = new StringBuilder();
        builder.Append('"');
        foreach (char c in value)
        {
            builder.Append(c switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                _ => c.ToString(),
            });
        }
        builder.Append('"');
        return builder.ToString();
    }

    private static string GetTagOrDefault(DefTypeBase defType, string name, string defaultValue)
    {
        string value = defType.GetTag(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    private static bool GetBoolTag(DefTypeBase defType, string name, bool defaultValue)
    {
        string value = defType.GetTag(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : ParseBool(value, name);
    }

    private static string GetOption(string name, string defaultValue)
    {
        return EnvManager.Current?.GetOptionOrDefaultRaw(name, defaultValue) ?? defaultValue;
    }

    private static bool GetBoolOption(string name, bool defaultValue)
    {
        string value = EnvManager.Current?.GetOptionRaw(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : ParseBool(value, name);
    }

    private static bool ParseBool(string value, string name)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "1" or "true" or "yes" or "on" => true,
            "0" or "false" or "no" or "off" => false,
            _ => throw new Exception($"invalid bool value for '{name}': {value}"),
        };
    }
}
