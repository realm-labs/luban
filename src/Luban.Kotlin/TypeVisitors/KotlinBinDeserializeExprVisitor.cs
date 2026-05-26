using Luban.Types;
using Luban.TypeVisitors;
using Luban.Utils;

namespace Luban.Kotlin.TypeVisitors;

public class KotlinBinDeserializeExprVisitor : DecoratorFuncVisitor<string, string>
{
    public static KotlinBinDeserializeExprVisitor Ins { get; } = new();

    public override string DoAccept(TType type, string bufName)
    {
        string value = type.Apply(KotlinBinUnderlyingDeserializeExprVisitor.Ins, bufName);
        return type.IsNullable ? $"if ({bufName}.readBool()) {value} else null" : value;
    }
}

public class KotlinBinUnderlyingDeserializeExprVisitor : ITypeFuncVisitor<string, string>
{
    public static KotlinBinUnderlyingDeserializeExprVisitor Ins { get; } = new();

    public string Accept(TBool type, string bufName) => $"{bufName}.readBool()";

    public string Accept(TByte type, string bufName) => $"{bufName}.readByte()";

    public string Accept(TShort type, string bufName) => $"{bufName}.readShort()";

    public string Accept(TInt type, string bufName) => $"{bufName}.readInt()";

    public string Accept(TLong type, string bufName) => $"{bufName}.readLong()";

    public string Accept(TFloat type, string bufName) => $"{bufName}.readFloat()";

    public string Accept(TDouble type, string bufName) => $"{bufName}.readDouble()";

    public string Accept(TEnum type, string bufName)
    {
        string src = $"{bufName}.readInt()";
        string constructor = type.DefEnum.TypeConstructorWithTypeMapper();
        if (!string.IsNullOrEmpty(constructor))
        {
            return $"{constructor}({src})";
        }
        return $"{type.DefEnum.FullNameWithTopModule}.fromValue({src})";
    }

    public string Accept(TString type, string bufName) => $"{bufName}.readString()";

    public string Accept(TDateTime type, string bufName) => $"{bufName}.readLong()";

    public string Accept(TBean type, string bufName)
    {
        string src = $"{type.DefBean.FullNameWithTopModule}.deserialize({bufName})";
        string constructor = type.DefBean.TypeConstructorWithTypeMapper();
        return string.IsNullOrEmpty(constructor) ? src : $"{constructor}({src})";
    }

    public string Accept(TArray type, string bufName) => ReadList(type.ElementType, bufName, "array");

    public string Accept(TList type, string bufName) => ReadList(type.ElementType, bufName, "list");

    public string Accept(TSet type, string bufName)
    {
        return $"{ReadList(type.ElementType, bufName, "set")}.toSet()";
    }

    public string Accept(TMap type, string bufName)
    {
        string keyType = type.KeyType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins);
        string valueType = type.ValueType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins);
        string keyExpr = type.KeyType.Apply(KotlinBinDeserializeExprVisitor.Ins, bufName);
        string valueExpr = type.ValueType.Apply(KotlinBinDeserializeExprVisitor.Ins, bufName);
        return $"run {{ val __n = {bufName}.readSize(); val __m = LinkedHashMap<{keyType}, {valueType}>(__n); repeat(__n) {{ val __k = {keyExpr}; val __v = {valueExpr}; __m[__k] = __v }}; __m }}";
    }

    private static string ReadList(TType elementType, string bufName, string prefix)
    {
        string element = elementType.Apply(KotlinBinDeserializeExprVisitor.Ins, bufName);
        return $"run {{ val __n = {bufName}.readSize(); List(__n) {{ {element} }} }}";
    }
}
