namespace Infra.Net.Extensions.Helpers.Domain;

public static class CpfHelper
{
    private static string GerarDigito(string cpf)
    {
        var peso = 2;
        var soma = 0;

        for (var i = cpf.Length - 1; i >= 0; i--)
        {
            soma += peso * Convert.ToInt32(cpf[i].ToString());
            peso++;
        }

        var pNumero = 11 - (soma % 11);

        if (pNumero > 9)
            pNumero = 0;

        return pNumero.ToString();
    }


    public static bool Validar(string cpf)
    {
        if (cpf.IsNullOrWhiteSpace())
            return false;

        var aux = cpf.ExtractNumbers().PadLeft(11, '0');

        if (aux.Length != 11)
            return false;

        for (var number = 0; number < 10; number++)
        {
            if (aux == new String(number.ToString()[0], 11))
                return false;
        }

        var pDigito = aux.Substring(9, 2);
        aux = aux[..9];

        aux += GerarDigito(aux);

        aux += GerarDigito(aux);

        return pDigito == aux.Substring(9, 2);
    }
}