using Luban.Types;
using Luban.TypeVisitors;
using Luban.Utils;

namespace Luban.Kotlin.TypeVisitors;

public class KotlinJsonDeserializeExprVisitor : DecoratorFuncVisitor<string, string>
{
    public static KotlinJsonDeserializeExprVisitor Ins { get; } = new();

    public override string DoAccept(TType type, string jsonName)
    {
        string value = type.Apply(KotlinJsonUnderlyingDeserializeExprVisitor.Ins, jsonName);
        return type.IsNullable ? $"if ({jsonName} is JsonNull) null else {value}" : value;
    }
}

public class KotlinJsonUnderlyingDeserializeExprVisitor : ITypeFuncVisitor<string, string>
{
    public static KotlinJsonUnderlyingDeserializeExprVisitor Ins { get; } = new();

    public string Accept(TBool type, string jsonName) => $"{jsonName}.jsonPrimitive.boolean";

    public string Accept(TByte type, string jsonName) => $"{jsonName}.jsonPrimitive.int.toByte()";

    public string Accept(TShort type, string jsonName) => $"{jsonName}.jsonPrimitive.int.toShort()";

    public string Accept(TInt type, string jsonName) => $"{jsonName}.jsonPrimitive.int";

    public string Accept(TLong type, string jsonName) => $"{jsonName}.jsonPrimitive.long";

    public string Accept(TFloat type, string jsonName) => $"{jsonName}.jsonPrimitive.float";

    public string Accept(TDouble type, string jsonName) => $"{jsonName}.jsonPrimitive.double";

    public string Accept(TEnum type, string jsonName)
    {
        string src = $"{jsonName}.jsonPrimitive.int";
        string constructor = type.DefEnum.TypeConstructorWithTypeMapper();
        if (!string.IsNullOrEmpty(constructor))
        {
            return $"{constructor}({src})";
        }
        return $"{type.DefEnum.FullNameWithTopModule}.fromValue({src})";
    }

    public string Accept(TString type, string jsonName) => $"{jsonName}.jsonPrimitive.content";

    public string Accept(TDateTime type, string jsonName) => $"{jsonName}.jsonPrimitive.long";

    public string Accept(TBean type, string jsonName)
    {
        string src = $"{type.DefBean.FullNameWithTopModule}.deserialize({jsonName}.jsonObject)";
        string constructor = type.DefBean.TypeConstructorWithTypeMapper();
        return string.IsNullOrEmpty(constructor) ? src : $"{constructor}({src})";
    }

    public string Accept(TArray type, string jsonName) => ReadList(type.ElementType, jsonName);

    public string Accept(TList type, string jsonName) => ReadList(type.ElementType, jsonName);

    public string Accept(TSet type, string jsonName) => $"{ReadList(type.ElementType, jsonName)}.toSet()";

    public string Accept(TMap type, string jsonName)
    {
        string keyExpr = type.KeyType.Apply(KotlinJsonDeserializeExprVisitor.Ins, "__entry[0]");
        string valueExpr = type.ValueType.Apply(KotlinJsonDeserializeExprVisitor.Ins, "__entry[1]");
        return $"{jsonName}.jsonArray.associate {{ val __entry = it.jsonArray; {keyExpr} to {valueExpr} }}";
    }

    private static string ReadList(TType elementType, string jsonName)
    {
        string element = elementType.Apply(KotlinJsonDeserializeExprVisitor.Ins, "it");
        return $"{jsonName}.jsonArray.map {{ {element} }}";
    }
}
