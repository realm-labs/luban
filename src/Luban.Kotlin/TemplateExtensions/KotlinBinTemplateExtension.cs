using Luban.Kotlin.TypeVisitors;
using Luban.Types;
using Scriban.Runtime;

namespace Luban.Kotlin.TemplateExtensions;

public class KotlinBinTemplateExtension : ScriptObject
{
    public static string Deserialize(string bufName, TType type)
    {
        return type.Apply(KotlinBinDeserializeExprVisitor.Ins, bufName);
    }
}
