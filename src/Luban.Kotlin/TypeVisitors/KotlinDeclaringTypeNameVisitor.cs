using Luban.Types;
using Luban.TypeVisitors;
using Luban.Utils;

namespace Luban.Kotlin.TypeVisitors;

public class KotlinDeclaringTypeNameVisitor : ITypeFuncVisitor<string>
{
    public static KotlinDeclaringTypeNameVisitor Ins { get; } = new();

    protected virtual bool BoxPrimitives => false;

    protected string Nullable(TType type, string name)
    {
        return type.IsNullable ? $"{name}?" : name;
    }

    public virtual string Accept(TBool type) => Nullable(type, "Boolean");

    public virtual string Accept(TByte type) => Nullable(type, "Byte");

    public virtual string Accept(TShort type) => Nullable(type, "Short");

    public virtual string Accept(TInt type) => Nullable(type, "Int");

    public virtual string Accept(TLong type) => Nullable(type, "Long");

    public virtual string Accept(TFloat type) => Nullable(type, "Float");

    public virtual string Accept(TDouble type) => Nullable(type, "Double");

    public virtual string Accept(TEnum type)
    {
        string typeName = type.DefEnum.TypeNameWithTypeMapper() ?? type.DefEnum.FullNameWithTopModule;
        return Nullable(type, typeName);
    }

    public string Accept(TString type) => Nullable(type, "String");

    public virtual string Accept(TDateTime type) => Nullable(type, "Long");

    public string Accept(TBean type)
    {
        string typeName = type.DefBean.TypeNameWithTypeMapper() ?? type.DefBean.FullNameWithTopModule;
        return Nullable(type, typeName);
    }

    public string Accept(TArray type) => Nullable(type, $"List<{type.ElementType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins)}>");

    public string Accept(TList type) => Nullable(type, $"List<{type.ElementType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins)}>");

    public string Accept(TSet type) => Nullable(type, $"Set<{type.ElementType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins)}>");

    public string Accept(TMap type)
    {
        return Nullable(type, $"Map<{type.KeyType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins)}, {type.ValueType.Apply(KotlinDeclaringBoxTypeNameVisitor.Ins)}>");
    }
}

public class KotlinDeclaringBoxTypeNameVisitor : KotlinDeclaringTypeNameVisitor
{
    public new static KotlinDeclaringBoxTypeNameVisitor Ins { get; } = new();
}
