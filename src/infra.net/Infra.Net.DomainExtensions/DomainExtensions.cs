
namespace Infra.Net.DomainExtensions;

public static class DomainExtensions
{

    [DebuggerStepThrough]
    public static string FormatCpfOrCnpj(this string source)
    {
        if (source.IsCNPJ())
            return FormatCNPJ(source);
        if (source.IsCPF())
            return FormatCPF(source);

        return source;
    }

    [DebuggerStepThrough]
    public static string MaskCpfOrCnpj(this string source)
    {
        if (source.IsCNPJ())
            return MaskCNPJ(source);
        if (source.IsCPF())
            return MaskCPF(source);

        return source;
    }

    [DebuggerStepThrough]
    public static string FormatCPF(this string source)
    {
        if (source.IsEmpty()) return source;
        var cpf = long.Parse(source.ExtractNumbers());
        return $@"{cpf:000\.000\.000\-00}";
    }

    [DebuggerStepThrough]
    public static string MaskCPF(this string source)
    {
        if (source.IsEmpty()) return source;
        var originalSource = source.ExtractNumbers().PadLeft(11, '0');
        var unamaskedChars = originalSource[..3] + originalSource[8..];
        var maskedInt = unamaskedChars.ToInt();
        return $@"{maskedInt:000\.***\.**0\-00}";
    }

    [DebuggerStepThrough]
    public static string FormatCNPJ(this string source)
    {
        if (source.IsEmpty()) return source;
        var cnpj = long.Parse(source.ExtractNumbers());
        return $@"{cnpj:00\.000\.000\/0000\-00}";
    }

    [DebuggerStepThrough]
    public static string MaskCNPJ(this string source)
    {
        if (source.IsEmpty()) return source;
        var originalSource = source.ExtractNumbers().PadLeft(14, '0');
        var unmaskedChars = originalSource[..3] + originalSource[8..];
        var maskedInt = unmaskedChars.ToInt();
        return $@"{maskedInt:00\.0**\.***\/0000\-00}";
    }

    [DebuggerStepThrough]
    public static string FormatCep(this string source)
    {
        if (source.IsEmpty()) return source;
        var cep = long.Parse(source.ExtractNumbers());
        return $@"{cep:00000\-000}";
    }

    [DebuggerStepThrough]
    public static bool IsCep(this string value)
    {
        return !value.IsEmpty()
               && Regex.IsMatch(value, @"(^\d{2}[.]?\d{3}[-]?\d{3}$)");
    }

    [DebuggerStepThrough]
    public static bool IsCPF(this string source)
    {
        return CpfHelper.Validar(source);
    }

    [DebuggerStepThrough]
    public static bool IsCNPJ(this string source)
    {
        return CnpjHelper.Validar(source);
    }

    [DebuggerStepThrough]
    public static bool IsCpfOrCnpj(this string source)
    {
        return source.IsCNPJ() || source.IsCPF();
    }

    public static bool IsPIS(this string pis)
    {
        return PisHelper.Validar(pis);
    }
    [DebuggerStepThrough]
    public static string MaskEmail(this string email, bool fixedLength = false)
    {
        if (fixedLength)
            return MaskEmailFixedLength(email);

        var atIndex = email.IndexOf('@') + 1;
        var firstDotIndex = email.IndexOf('.') + 1;
        var username = MaskPiece(email[..(atIndex - 1)]);
        var domain = MaskPiece(email.Substring(atIndex, firstDotIndex - 1 - atIndex));
        var suffixes = email[firstDotIndex..].Split('.');
        var suffix = string.Join(".", suffixes.Select(MaskPiece));

        return $"{username}@{domain}.{suffix}";
    }

    private static string MaskPiece(string piece)
    {
        if (piece.Length == 1)
            piece = "*";
        if (piece.Length == 2)
            piece = $"{piece[..1]}*";
        if (piece.Length >= 3)
            piece = piece[..1]
                        .PadRight(piece.Length - 1, '*') +
                    piece[^1..];
        return piece;
    }

    [DebuggerStepThrough]
    public static string MaskEmailFixedLength(this string email)
    {
        return Regex.Replace(email ?? string.Empty,
            "(^.{1,2})(?:.*?)?(.?\\@.?)(?:.*?)?(.\\.)(?:.*?)?(.\\.)?(?:.)?(.$)", "$1****$2***$3**$4*$5");
    }

    [DebuggerStepThrough]
    public static string MaskNumber(this string number, int expectedLength)
    {
        var length = expectedLength > number.Length ? expectedLength : number.Length;
        number = number.PadLeft(length, '0');

        switch (number.Length)
        {
            case 1:
                return "*";
            case 2:
                return $"{number[..1]}*";
            case int a when a <= 5:
                return $"{number[..1]}" +
                       (number.Length - 2).Times('*') +
                       $"{number[^1..]}";
            default:
                return $"{number[..2]}" +
                       (number.Length - 4).Times('*') +
                       $"{number[^2..]}";
        }
    }
}