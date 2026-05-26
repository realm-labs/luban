using Luban.Kotlin.TypeVisitors;
using Luban.Types;
using Scriban.Runtime;

namespace Luban.Kotlin.TemplateExtensions;

public class KotlinJsonTemplateExtension : ScriptObject
{
    public static string Deserialize(string jsonVar, TType type)
    {
        return type.Apply(KotlinJsonDeserializeExprVisitor.Ins, jsonVar);
    }

    public static string DeserializeField(string jsonName, string ownerName, string jsonFieldName, TType type)
    {
        return $"JsonUtil.readField({jsonName}, \"{Escape(ownerName)}\", \"{Escape(jsonFieldName)}\") {{ __jsonValue -> {type.Apply(KotlinJsonDeserializeExprVisitor.Ins, "__jsonValue")} }}";
    }

    public static string DeserializeNullableField(string jsonName, string ownerName, string jsonFieldName, TType type)
    {
        return $"JsonUtil.readNullableField({jsonName}, \"{Escape(ownerName)}\", \"{Escape(jsonFieldName)}\") {{ __jsonValue -> {type.Apply(KotlinJsonUnderlyingDeserializeExprVisitor.Ins, "__jsonValue")} }}";
    }

    public static string DeserializeTypeField(string jsonName, string ownerName)
    {
        return $"JsonUtil.readField({jsonName}, \"{Escape(ownerName)}\", \"\\$type\") {{ __jsonValue -> __jsonValue.jsonPrimitive.content }}";
    }

    private static string Escape(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
