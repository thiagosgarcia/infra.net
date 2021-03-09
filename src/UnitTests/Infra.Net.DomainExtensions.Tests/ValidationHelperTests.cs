using Infra.Net.DomainExtensions;
using Infra.Net.Extensions.Extensions;
using Xunit;
using Xunit2.Should;

namespace Infra.Net.Extensions.Tests.Helpers
{
    public class ValidationHelperTests
    {
        [Theory]
        [InlineData("36.320.880/0001-20")]
        [InlineData("88.143.821/0001-28")]
        [InlineData("67.232.732/0001-88")]
        public void MustCnpjValidWithMask(string cnpjTest)
        {
            cnpjTest.IsCNPJ().ShouldBeTrue();
            cnpjTest.IsCpfOrCnpj().ShouldBeTrue();
        }

        [Theory]
        [InlineData("76748996000180")]
        [InlineData("80836586000168")]
        [InlineData("87738597000154")]
        public void MustCnpjValidJustNumbers(string cnpjTest)
        {
            cnpjTest.IsCNPJ().ShouldBeTrue();
            cnpjTest.IsCpfOrCnpj().ShouldBeTrue();
        }
        [Theory]
        [InlineData("36.320.880/0001-20")]
        [InlineData("88.143.821/0001-28")]
        [InlineData("67.232.732/0001-88")]
        [InlineData("")]
        public void MustFormatCnpj(string cnpj)
        {
            cnpj.ExtractNumbers().FormatCNPJ().ShouldBeEqual(cnpj);
        }

        [Theory]
        [InlineData("36.320.880/0001-20", "36.3**.***/0001-20")]
        [InlineData("88.143.821/0001-28", "88.1**.***/0001-28")]
        [InlineData("67.232.732/0001-88", "67.2**.***/0001-88")]
        [InlineData("", "")]
        public void MustMaskCnpj(string cnpj, string expected)
        {
            cnpj.ExtractNumbers().MaskCNPJ().ShouldBeEqual(expected);
        }

        [Theory]
        [InlineData("482.496.596-94", "482.***.**6-94")]
        [InlineData("948.843.518-60", "948.***.**8-60")]
        [InlineData("689.173.690-06", "689.***.**0-06")]
        [InlineData("36.320.880/0001-20", "36.3**.***/0001-20")]
        [InlineData("88.143.821/0001-28", "88.1**.***/0001-28")]
        [InlineData("67.232.732/0001-88", "67.2**.***/0001-88")]
        [InlineData("", "")]
        public void ShouldMaskCpfOrCnpj(string item, string expected)
        {
            item.ExtractNumbers().MaskCpfOrCnpj().ShouldBeEqual(expected);
        }

        [Theory]
        [InlineData("70.555.555/0001-93")]
        [InlineData("34.43.64/0001-64")]
        [InlineData("65.378.172@0001@049")]
        [InlineData("55.555.555/5555-55")]
        [InlineData("")]
        [InlineData(null)]
        public void MustCnpjInvalidWithMask(string cnpjTest)
        {
            cnpjTest.IsCNPJ().ShouldBeFalse();
            cnpjTest.IsCpfOrCnpj().ShouldBeFalse();
        }

        [Theory]
        [InlineData("814547000178")]
        [InlineData("520555555787000119")]
        [InlineData("42693367999185")]
        [InlineData("55555555555")]
        [InlineData("")]
        [InlineData(null)]
        public void MustCnpjInvalidJustNumbers(string cnpjTest)
        {
            cnpjTest.IsCNPJ().ShouldBeFalse();
            cnpjTest.IsCpfOrCnpj().ShouldBeFalse();
        }

        [Theory]
        [InlineData("482.496.596-94")]
        [InlineData("948.843.518-60")]
        [InlineData("689.173.690-06")]
        [InlineData("053.178.159-32")]
        public void MustCpfValidWithMask(string cpfTest)
        {
            cpfTest.IsCPF().ShouldBeTrue();
            cpfTest.IsCpfOrCnpj().ShouldBeTrue();
        }

        [Theory]
        [InlineData("482.496.596-94", "482.***.**6-94")]
        [InlineData("948.843.518-60", "948.***.**8-60")]
        [InlineData("689.173.690-06", "689.***.**0-06")]
        [InlineData("053.178.159-32", "053.***.**9-32")]
        [InlineData("", "")]
        public void ShouldFormatCpfWithMask(string cpf, string expected)
        {
            cpf.ExtractNumbers().MaskCPF().ShouldBeEqual(expected);
        }
        [Theory]
        [InlineData("482.496.596-94")]
        [InlineData("948.843.518-60")]
        [InlineData("689.173.690-06")]
        [InlineData("053.178.159-32")]
        [InlineData("")]
        public void MustFormatCpf(string cpf)
        {
            cpf.ExtractNumbers().FormatCPF().ShouldBeEqual(cpf);
        }
        [Theory]
        [InlineData("482.496.596-94")]
        [InlineData("948.843.518-60")]
        [InlineData("689.173.690-06")]
        [InlineData("36.320.880/0001-20")]
        [InlineData("88.143.821/0001-28")]
        [InlineData("67.232.732/0001-88")]
        [InlineData("")]
        public void MustFormatCpfOrCnpj(string item)
        {
            item.ExtractNumbers().FormatCpfOrCnpj().ShouldBeEqual(item);
        }

        [Theory]
        [InlineData("16812192635")]
        [InlineData("65325255290")]
        [InlineData("26954228818")]
        [InlineData("05317815932")]
        [InlineData("4622143623")]
        [InlineData("04622143623")]
        public void MustCpfValidJustNumbers(string cpfTest)
        {
            cpfTest.IsCPF().ShouldBeTrue();
            cpfTest.IsCpfOrCnpj().ShouldBeTrue();
        }

        [Theory]
        [InlineData("373.871.-97")]
        [InlineData("121.164.530")]
        [InlineData("121.567.530-99")]
        [InlineData("921.....530-ER")]
        [InlineData("")]
        [InlineData(null)]
        public void MustCpfInvalidWithMask(string cpfTest)
        {
            cpfTest.IsCPF().ShouldBeFalse();
            cpfTest.IsCpfOrCnpj().ShouldBeFalse();
        }

        [Theory]
        [InlineData("152892652")]
        [InlineData("28134617412")]
        [InlineData("78 95 13 16 50")]
        [InlineData("")]
        [InlineData(null)]
        public void MustCpfInvalidJustNumbers(string cpfTest)
        {
            cpfTest.IsCPF().ShouldBeFalse();
            cpfTest.IsCpfOrCnpj().ShouldBeFalse();
        }

        [Theory]
        [InlineData("219.77689.19-3")]
        [InlineData("040.78525.77-5")]
        [InlineData("511.90657.13-2")]
        public void MustPisValidWithMask(string pisTest)
        {
            pisTest.IsPIS().ShouldBeTrue();
        }

        [Theory]
        [InlineData("78807027118")]
        [InlineData("27944141090")]
        [InlineData("32216633212")]
        public void MustPisValidJustNumbers(string pisTest)
        {
            pisTest.IsPIS().ShouldBeTrue();
        }

        [Theory]
        [InlineData("939.78484.71-$")]
        [InlineData("965.24304.36-4785")]
        [InlineData("21.15452.85-0")]
        [InlineData("226.59272.52-2")]
        public void MustPisInvalidWithMask(string pisTest)
        {
            pisTest.IsPIS().ShouldBeFalse();
        }

        [Theory]
        [InlineData("65171159888")]
        [InlineData("7138525236")]
        [InlineData("285911108065")]
        public void MustPisInvalidJustNumbers(string pisTest)
        {
            pisTest.IsPIS().ShouldBeFalse();
        }

        [Theory]
        [InlineData("27410280", "27410-280")]
        [InlineData("80230010", "80230-010")]
        [InlineData("", "")]
        public void MustFormatCep(string value, string cep)
        {
            value.FormatCep().ShouldBeEqual(cep);
        }

        [Theory]
        [InlineData("27410-280")]
        [InlineData("27410280")]
        [InlineData("80230-010")]
        [InlineData("80230010")]
        public void MustValidateCep(string cep)
        {
            cep.IsCep().ShouldBeTrue();
        }

        [Theory]
        [InlineData("2741a-280")]
        [InlineData("2741a280")]
        [InlineData("802300-010")]
        [InlineData("802300010")]
        public void MustNotValidateCep(string cep)
        {
            cep.IsCep().ShouldBeFalse();
        }


        [Theory]
        [InlineData("a@d.co.uk", "*@*.c*.u*")]
        [InlineData("a@d.co", "*@*.c*")]
        [InlineData("a@domain.com", "*@d****n.c*m")]
        [InlineData("ab@domain.com", "a*@d****n.c*m")]
        [InlineData("abc@domain.com", "a*c@d****n.c*m")]
        [InlineData("abcd@domain.com", "a**d@d****n.c*m")]
        [InlineData("teste@domain.com", "t***e@d****n.c*m")]
        [InlineData("cantina_da_tia+zefa@dominio.net.br", "c*****************a@d*****o.n*t.b*")]
        public void MustMaskEmail(string email, string expected)
        {
            email.MaskEmail().ShouldBeEqual(expected);
        }

        [Theory]
        [InlineData("a@d.co.uk", "a****@d***o.***k")]
        [InlineData("a@d.co", "a****@***d.***o")]
        [InlineData("a@domain.com", "a****@d***n.***m")]
        [InlineData("ab@domain.com", "ab****@d***n.***m")]
        [InlineData("abc@domain.com", "ab****c@d***n.***m")]
        [InlineData("abcd@domain.com", "ab****d@d***n.***m")]
        [InlineData("teste@domain.com", "te****e@d***n.***m")]
        [InlineData("cantina_da_tia+zefa@dominio.net.br", "ca****a@d***o.**t.*r")]
        public void MustMaskEmailFixedLength(string email, string expectedFixedLength)
        {
            email.MaskEmail(true).ShouldBeEqual(expectedFixedLength);
        }

        [Theory]
        [InlineData(12345, 9, "00*****45")]
        [InlineData(123456789, 9, "12*****89")]
        [InlineData(123456789, 14, "00**********89")]
        [InlineData(123, 4, "0**3")]
        [InlineData(1, 1, "*")]
        [InlineData(1, 10, "00******01")]
        [InlineData(12, 2, "1*")]
        [InlineData(123, 2, "1*3")]
        [InlineData(1234, 2, "1**4")]
        [InlineData(12345, 2, "1***5")]
        [InlineData(123456, 2, "12**56")]
        [InlineData(1234567, 2, "12***67")]
        [InlineData(12345678, 2, "12****78")]
        public void MustMaskNumber(int anyNumber, int expectedLength, string expected)
        {
            anyNumber.ToString().MaskNumber(expectedLength).ShouldBeEqual(expected);
        }

    }
}
