using System.IO;
using System.Xml.Serialization;
using Infra.Net.Extensions.Extensions;
using Infra.Net.Extensions.Xml;
using Xunit;
using Xunit2.Should;

namespace Infra.Net.Extensions.Tests.Extensions
{
    public class ObjectExtensionsTests
    {

        [Fact]
        public void ToXmlTest()
        {
            var numContrato = "1009579950";
            var xmlControle = montarXML(numContrato);

            var contratoDados = new ContratoDados
            {
                TipoDeConsulta = "fat_gen_faturamento.get_rsdadoscontrato",
                NumeroDoContrato = numContrato
            };

            var xmlExtensionSettings = new XmlExtensionSettings
            {
                OmitXmlDeclaration = false,
                StandAlone = true
            };

            var xmlProposto = contratoDados.ToXml(xmlExtensionSettings);

            var serializer = new XmlSerializer(typeof(ContratoDados));
            var objetoControle = new ContratoDados();
            var objetoProposto = new ContratoDados();

            using (TextReader reader = new StringReader(xmlProposto))
            {
                objetoProposto = (ContratoDados)serializer.Deserialize(reader);
            }

            using (TextReader reader = new StringReader(xmlControle))
            {
                objetoControle = (ContratoDados)serializer.Deserialize(reader);
            }

            Assert.Equal(objetoProposto.NumeroDoContrato, objetoControle.NumeroDoContrato);
        }

        private string montarXML(string numContrato)
        {
            string retVal =
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
                     <consulta>
                        <tipoconsulta>fat_gen_faturamento.get_rsdadoscontrato</tipoconsulta>
                        <num_contrato>{0}</num_contrato>
                     </consulta>";
            return string.Format(retVal, numContrato);
        }
        [Fact]
        public void MustBeNullOrDefaultDefaultValue()
        {
            0.IsNullOrDefault().ShouldBeTrue();
            ((uint)0).IsNullOrDefault().ShouldBeTrue();
            ((long)0).IsNullOrDefault().ShouldBeTrue();
            ((ulong)0).IsNullOrDefault().ShouldBeTrue();
            ((short)0).IsNullOrDefault().ShouldBeTrue();
            ((ushort)0).IsNullOrDefault().ShouldBeTrue();
            ((byte)0).IsNullOrDefault().ShouldBeTrue();
            ((sbyte)0).IsNullOrDefault().ShouldBeTrue();

            ((double)0).IsNullOrDefault().ShouldBeTrue();
            ((float)0).IsNullOrDefault().ShouldBeTrue();
            ((char)0).IsNullOrDefault().ShouldBeTrue();
            ((decimal)0).IsNullOrDefault().ShouldBeTrue();
        }
        [Fact]
        public void MustBeNullOrDefaultNullValue()
        {
            ((int?)null).IsNullOrDefault().ShouldBeTrue();
            ((uint?)null).IsNullOrDefault().ShouldBeTrue();
            ((long?)null).IsNullOrDefault().ShouldBeTrue();
            ((ulong?)null).IsNullOrDefault().ShouldBeTrue();
            ((short?)null).IsNullOrDefault().ShouldBeTrue();
            ((ushort?)null).IsNullOrDefault().ShouldBeTrue();
            ((byte?)null).IsNullOrDefault().ShouldBeTrue();
            ((sbyte?)null).IsNullOrDefault().ShouldBeTrue();

            ((double?)null).IsNullOrDefault().ShouldBeTrue();
            ((float?)null).IsNullOrDefault().ShouldBeTrue();
            ((char?)null).IsNullOrDefault().ShouldBeTrue();
            ((decimal?)null).IsNullOrDefault().ShouldBeTrue();
        }
        [Theory]
        [InlineData(-1)]
        [InlineData(1)]
        public void MustNotBeNullOrDefault(int val)
        {
            val.IsNullOrDefault().ShouldBeFalse();
            ((uint)val).IsNullOrDefault().ShouldBeFalse();
            ((long)val).IsNullOrDefault().ShouldBeFalse();
            ((ulong)val).IsNullOrDefault().ShouldBeFalse();
            ((short)val).IsNullOrDefault().ShouldBeFalse();
            ((ushort)val).IsNullOrDefault().ShouldBeFalse();
            ((byte)val).IsNullOrDefault().ShouldBeFalse();
            ((sbyte)val).IsNullOrDefault().ShouldBeFalse();

            ((double)val).IsNullOrDefault().ShouldBeFalse();
            ((float)val).IsNullOrDefault().ShouldBeFalse();
            ((char)val).IsNullOrDefault().ShouldBeFalse();
            ((decimal)val).IsNullOrDefault().ShouldBeFalse();
        }
    }

    [XmlRoot("consulta")]
    public class ContratoDados
    {
        [XmlElement("tipoconsulta")]
        public string TipoDeConsulta
        { get; set; }

        [XmlElement("num_contrato")]
        public string NumeroDoContrato
        { get; set; }
    }
}
