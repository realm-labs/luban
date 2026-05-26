using Luban.CodeFormat;
using Luban.CodeTarget;
using Luban.Kotlin.TemplateExtensions;
using Scriban;

namespace Luban.Kotlin.CodeTarget;

public abstract class KotlinCodeTargetBase : TemplateCodeTargetBase
{
    public override string FileHeader => CommonFileHeaders.AUTO_GENERATE_C_LIKE;

    protected override string FileSuffixName => "kt";

    protected override string CommonTemplateSearchPath => "common/kotlin";

    protected override ICodeStyle DefaultCodeStyle => CodeFormatManager.Ins.JavaDefaultCodeStyle;

    private static readonly HashSet<string> s_preservedKeyWords = new()
    {
        "as", "break", "class", "continue", "do", "else", "false", "for", "fun", "if", "in", "interface",
        "is", "null", "object", "package", "return", "super", "this", "throw", "true", "try", "typealias",
        "typeof", "val", "var", "when", "while"
    };

    protected override IReadOnlySet<string> PreservedKeyWords => s_preservedKeyWords;

    protected override string GetFileNameWithoutExtByTypeName(string name)
    {
        return name.Replace('.', '/');
    }

    protected override void OnCreateTemplateContext(TemplateContext ctx)
    {
        ctx.PushGlobal(new KotlinCommonTemplateExtension());
    }
}
