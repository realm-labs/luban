using Luban.CodeFormat;
using Luban.CodeFormat.CodeStyles;
using Luban.CodeTarget;
using Luban.Kotlin.TemplateExtensions;
using Scriban;
using System.Globalization;
using System.Text;

namespace Luban.Kotlin.CodeTarget;

public abstract class KotlinCodeTargetBase : TemplateCodeTargetBase
{
    public override string FileHeader => CommonFileHeaders.AUTO_GENERATE_C_LIKE;

    protected override string FileSuffixName => "kt";

    protected override string CommonTemplateSearchPath => "common/kotlin";

    protected override ICodeStyle DefaultCodeStyle => CodeFormatManager.Ins.JavaDefaultCodeStyle;

    protected override ICodeStyle CodeStyle => _kotlinCodeStyle ??= new KotlinCodeStyle(base.CodeStyle);

    private ICodeStyle _kotlinCodeStyle;

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

    private sealed class KotlinCodeStyle : CodeStyleBase
    {
        private readonly ICodeStyle _delegate;

        public KotlinCodeStyle(ICodeStyle @delegate)
        {
            _delegate = @delegate;
        }

        public override string FormatNamespace(string ns)
        {
            return SanitizeNamespace(_delegate.FormatNamespace(ns));
        }

        public override string FormatType(string typeName)
        {
            return SanitizeIdentifier(_delegate.FormatType(typeName));
        }

        public override string FormatMethod(string methodName)
        {
            return SanitizeIdentifier(_delegate.FormatMethod(methodName));
        }

        public override string FormatProperty(string propertyName)
        {
            return SanitizeIdentifier(_delegate.FormatProperty(propertyName));
        }

        public override string FormatField(string fieldName)
        {
            return SanitizeIdentifier(_delegate.FormatField(fieldName));
        }

        public override string FormatEnumItemName(string enumItemName)
        {
            return SanitizeIdentifier(_delegate.FormatEnumItemName(enumItemName));
        }

        private static string SanitizeIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var builder = new StringBuilder(name.Length);
            foreach (var c in name)
            {
                if (char.GetUnicodeCategory(c) != UnicodeCategory.Format)
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private static string SanitizeNamespace(string ns)
        {
            return string.Join(".", ns.Split('.').Select(SanitizeIdentifier));
        }
    }
}
