using System.Globalization;
using Infra.Net.Extensions.Extensions;
using Xunit;
using Xunit2.Should;

namespace Infra.Net.Extensions.Tests.Extensions;

public class StringExtensionTests
{
    [Theory]
    [InlineData("CamelHump", "camelhump")]
    [InlineData("CamelHump", "camelhuMP")]
    [InlineData("CamelHump", "CAMELHUMP")]
    [InlineData("CamelHump", "CamelHump")]
    public void ShouldEqualsIgnoreCase(string word, string value)
    {
        word.EqualsIgnoreCase(value).ShouldBeTrue();
    }

    [Theory]
    [InlineData("CamelHump", "CamelHum")]
    [InlineData("CamelHump", "CamelHump.")]
    [InlineData("CamelHump", "CamellHump")]
    public void ShouldNotEqualsIgnoreCase(string word, string value)
    {
        word.EqualsIgnoreCase(value).ShouldBeFalse();
    }
    [Theory]
    [InlineData("áãàÃóü", "ÂAãÁÔU")]
    [InlineData("áãàÃóü", "AAAaOU")]
    [InlineData("âmbito", "ÂMBITO")]
    [InlineData("Âmbito", "âMBITO")]
    [InlineData("AMBITO", "âmbito")]
    [InlineData("AMBITO", "ambito")]
    [InlineData("AMBITO", "AMBITO")]
    public void ShouldEqualsIgnoreCaseAccent(string word, string value)
    {
        word.EqualsIgnoreCaseAccent(value).ShouldBeTrue();
    }
    [Theory]
    [InlineData("áãàÃóü", "ÂAãeÔU")]
    [InlineData("âmbito", "ÂMBITOS")]
    [InlineData("Âmbitos", "âMBITO")]
    [InlineData("AMBITOs", "âmbito")]
    [InlineData("AMBITOS", "ambito")]
    [InlineData("AMBIT", "AMBITO")]
    public void ShouldNotEqualsIgnoreCaseAccent(string word, string value)
    {
        word.EqualsIgnoreCaseAccent(value).ShouldBeFalse();
    }

    [Theory]
    [InlineData("CamelHump", "camel")]
    [InlineData("CamelHump", "camelhum")]
    [InlineData("CamelHump", "CAMEL")]
    [InlineData("CamelHump", "CamELhuMp")]
    public void ShouldContainsIgnoreCase(string word, string value)
    {
        word.ContainsIgnoreCase(value).ShouldBeTrue();
    }

    [Theory]
    [InlineData("CamelHump", "Cem")]
    [InlineData("CamelHump", "CamelHump.")]
    [InlineData("CamelHump", "CamelHe")]
    [InlineData("CamelHump", "ump.")]
    public void ShouldNotContainsIgnoreCase(string word, string value)
    {
        word.ContainsIgnoreCase(value).ShouldBeFalse();
    }

    [Theory]
    [InlineData("CamelHump", "camel")]
    [InlineData("CamelHump", "camelhump")]
    [InlineData("CamelHump", "CAMEL")]
    [InlineData("CamelHump", "CamelHum")]
    public void ShouldStartsWithIgnoreCase(string word, string value)
    {
        word.StartsWithIgnoreCase(value).ShouldBeTrue();
    }

    [Theory]
    [InlineData("CamelHump", "Hum")]
    [InlineData("Camel", "CamelHump.")]
    [InlineData("Camel", "CamelHe")]
    [InlineData("Camel", "amelHump")]
    public void ShouldNotStartsWithIgnoreCase(string word, string value)
    {
        word.StartsWithIgnoreCase(value).ShouldBeFalse();
    }

    [Theory]
    [InlineData("Camel", "EL")]
    [InlineData("Camel", "el")]
    [InlineData("Camel", "MeL")]
    public void ShouldEndsWithIgnoreCase(string word, string value)
    {
        word.EndsWithIgnoreCase(value).ShouldBeTrue();
    }

    [Theory]
    [InlineData("Camel", "ca")]
    [InlineData("Camel", "ela")]
    [InlineData("Camel", "eel")]
    [InlineData("Camel", "cMEL")]
    public void ShouldNotEndsWithIgnoreCase(string word, string value)
    {
        word.EndsWithIgnoreCase(value).ShouldBeFalse();
    }

    [Theory]
    [InlineData("10", 10)]
    [InlineData("-98", -98)]
    public void MustConvertToInt(string toConvert, int expected)
    {
        var newInt = toConvert.ToInt();
        Assert.IsType<int>(newInt);
        newInt.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("1,5")]
    [InlineData("1a")]
    [InlineData("NaN")]
    public void MustThrowFormatExceptionWhenConvertingToInt(string toConvert)
    {
        Assert.Throws<FormatException>(() => toConvert.ToInt());
    }

    [Theory]
    [InlineData("1.5", 0)]
    [InlineData("1,5", 0)]
    [InlineData("1a", 1)]
    [InlineData("NaN", -100)]
    public void MustReturnDefaultValueWhenConvertingToInt(string toConvert, int value)
    {
        toConvert.ToInt(value).ShouldBe(value);
    }

    [Theory]
    [InlineData("-10", -10)]
    [InlineData("-98,3", -98.3)]
    [InlineData("1,8", 1.8)]
    public void MustConvertToDoubleWithPTCulture(string toConvert, double expected)
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt");
        var newDouble = toConvert.ToDouble();
        newDouble.ShouldBe(expected);
    }
    [Theory]
    [InlineData("-10", -10)]
    [InlineData("-98.3", -98.3)]
    [InlineData("1.8", 1.8)]
    public void MustConvertToDoubleWithENCulture(string toConvert, double expected)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en");
        var newDouble = toConvert.ToDouble();
        newDouble.ShouldBe(expected);
    }
    [Theory]
    [InlineData("-98.3", -98.3)]
    [InlineData("1.8", 1.8)]
    public void MustNotConvertToDoubleWithPTCulture(string toConvert, double expected)
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt");
        var newDouble = toConvert.ToDouble();
        newDouble.ShouldNotBe(expected);
    }
    [Theory]
    [InlineData("-98,3", -98.3)]
    [InlineData("1,8", 1.8)]
    public void MustNotConvertToDoubleWithENCulture(string toConvert, double expected)
    {
        CultureInfo.CurrentCulture = new CultureInfo("en");
        var newDouble = toConvert.ToDouble();
        newDouble.ShouldNotBe(expected);
    }

    [Theory]
    [InlineData("1a")]
    public void MustThrowFormatExceptionWhenConvertingToDouble(string toConvert)
    {
        Assert.Throws<FormatException>(() => toConvert.ToDouble());
    }

    [Theory]
    [InlineData("1a", 1)]
    [InlineData("nan", double.NaN)]
    public void MustReturnDefaultValueWhenConvertingToDouble(string toConvert, double value)
    {
        toConvert.ToDouble(value).ShouldBe(value);
    }

    [Fact]
    public void TestarToDouble()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt");
        var newDouble = "1,63".ToDouble();
        Assert.IsType<double>(newDouble);
        newDouble.ShouldBe(1.63d);

        Assert.Throws<FormatException>(() => "aaa".ToDouble(null));
        "aaa".ToDouble(1).ShouldBe(1d);
        "nan".ToDouble(1).ShouldBe(double.NaN);
    }

    [Theory]
    [InlineData("10", 10L)]
    [InlineData("-98", -98L)]
    public void MustConvertToLong(string toConvert, long expected)
    {
        var newLong = toConvert.ToLong();
        Assert.IsType<long>(newLong);
        newLong.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("1,5")]
    [InlineData("1a")]
    [InlineData("NaN")]
    public void MustThrowFormatExceptionWhenConvertingToLong(string toConvert)
    {
        Assert.Throws<FormatException>(() => toConvert.ToLong());
    }

    [Theory]
    [InlineData("1.5", 0L)]
    [InlineData("1,5", 0L)]
    [InlineData("1a", 1L)]
    [InlineData("NaN", -100L)]
    public void MustReturnDefaultValueWhenConvertingToLong(string toConvert, long value)
    {
        toConvert.ToLong(value).ShouldBe(value);
    }

    [Theory]
    [InlineData("2017-01-01", 2017, 01, 01)]
    [InlineData("2017-01-01T13:44:59", 2017,1,1,13,44,59)]
    [InlineData("05/01/2017",2017,01,05)]
    public void MustConvertToDateTimeTicks(string toConvert, int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt");
        var dateTimeTicks = toConvert.ToDateTimeTicks();
        Assert.IsType<long>(dateTimeTicks);
        var expectedDate = new DateTime(year, month, day, hour, minute, second);
        dateTimeTicks.ShouldBe(expectedDate.Ticks);
    }

    [Theory]
    [InlineData("2017-01-01", 2017, 01, 01)]
    [InlineData("2017-01-01T13:44:59", 2017, 1, 1, 13, 44, 59)]
    [InlineData("05/01/2017", 2017, 01, 05)]
    public void MustConvertToDateTime(string toConvert, int year, int month, int day, int hour = 0, int minute = 0,
        int second = 0)
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt");
        var dateTime = toConvert.ToDateTime();
        Assert.IsType<DateTime>(dateTime);
        var expectedDate = new DateTime(year, month, day, hour, minute, second);
        dateTime.ShouldBe(expectedDate);
    }

    [Theory]
    [InlineData("1a")]
    [InlineData("NaN")]
    public void MustThrowFormatExceptionWhenConvertingToDateTimeTicks(string toConvert)
    {
        Assert.Throws<FormatException>(() => toConvert.ToDateTimeTicks());
    }

    [Theory]
    [InlineData("separatedWords", "separated Words")]
    [InlineData("SeparatedWords", "Separated Words")]
    [InlineData("separated_Words", "separated Words")]
    [InlineData("separated_words", "separated words")]
    [InlineData("separated_wordsAnd-More words", "separated words And More words")]
    [InlineData("another-word", "another word")]
    [InlineData("two words", "two words")]
    public void MustConvertToSeparateWords(string value, string expected)
    {
        value.ToSeparatedWords().ShouldBeEqual(expected);
    }
        
    [Theory]
    [InlineData("ONEWORD")]
    [InlineData("oneword")]
    [InlineData("onew*ord")]
    [InlineData("onew/ord")]
    public void MustNotConvertToSeparateWords(string value)
    {
        value.ToSeparatedWords().ShouldBeEqual(value);
    }
        
    [Theory]
    [InlineData("ONEWORD", "Oneword")]
    [InlineData("oneword", "Oneword")]
    [InlineData("onew*ord", "Onew*ord")]
    [InlineData("onew/ord", "Onew/ord")]
    [InlineData("separatedWords", "SeparatedWords")]
    [InlineData("SeparatedWords", "SeparatedWords")]
    [InlineData("separated_Words", "SeparatedWords")]
    [InlineData("separated_words", "SeparatedWords")]
    [InlineData("separated_wordsAnd-More words", "SeparatedWordsAndMoreWords")]
    [InlineData("another-word", "AnotherWord")]
    [InlineData("two words", "TwoWords")]
    [InlineData("P_COD_USUARIO", "PCodUsuario")]
    [InlineData("COD_USUARIO", "CodUsuario")]
    [InlineData("P_COd_Usuario_nome", "PCodUsuarioNome")]
    public void MustConvertToPascalCase(string value, string expected)
    {
        value.ToPascalCase().ShouldBeEqual(expected);
    }
        
    [Theory]
    [InlineData("ONEWORD")]
    [InlineData("ALL10UPPER")]
    public void MustCheckUpper(string value)
    {
        value.IsAllUpper().ShouldBeTrue();
    }
        
    [Theory]
    [InlineData("ONeWORD")]
    [InlineData("ALL10uPPER")]
    public void MustNotCheckUpper(string value)
    {
        value.IsAllUpper().ShouldBeFalse();
    }
        
    [Theory]
    [InlineData("oneword")]
    [InlineData("all10lower")]
    public void MustCheckLower(string value)
    {
        value.IsAllLower().ShouldBeTrue();
    }
        
    [Theory]
    [InlineData("Oneword")]
    [InlineData("all10loweR")]
    public void MustNotCheckLower(string value)
    {
        value.IsAllLower().ShouldBeFalse();
    }
        
    [Theory]
    [InlineData("00a1d2f3gg456")]
    [InlineData("001d2f3gg456")]
    [InlineData("0a_0d1d2f3gg456")]
    public void MustExtractNumbers(string value)
    {
        value.ExtractNumbers().ShouldBeEqual("00123456");
    }
        
    [Theory]
    [InlineData("áéèàÀÈÁÉ", "aeeaAEAE")]
    [InlineData("áéèàôoÔOÀÈÁÉ", "aeeaooOOAEAE")]
    [InlineData("çÇCc", "cCCc")]
    [InlineData("ão12", "ao12")]
    public void MustNormalizeString(string value, string expected)
    {
        value.ToNormalForm().ShouldBeEqual(expected);
    }

    [Theory]
    [InlineData("a1d2f3gg456-)_2")]
    [InlineData("a1d2f3gg456*2")]
    public void MustExtractLettersAndNumbers(string value)
    {
        value.ExtractLettersAndNumbers().ShouldBeEqual("a1d2f3gg4562");
    }

    [Theory]
    [InlineData("a1d2f3gg456-)_2")]
    [InlineData("a1d2f3gg456")]
    public void MustExtractLEtters(string value)
    {
        value.ExtractLetters().ShouldBeEqual("adfgg");
    }

    [Theory]
    [InlineData("ab", 2, "abab")]
    [InlineData("ab", 1, "ab")]
    [InlineData("ab", 0, "")]
    [InlineData("ab", 5, "ababababab")]
    public void MustRepeatExactlyNumberOfTimesStringInput(string input, int count, string expected)
    {
        count.Times(input).ShouldBeEqual(expected);
    }

    [Theory]
    [InlineData('a', 2, "aa")]
    [InlineData('b', 1, "b")]
    [InlineData('c', 0, "")]
    [InlineData('d', 5, "ddddd")]
    public void MustRepeatExactlyNumberOfTimesCharInput(char input, int count, string expected)
    {
        count.Times(input).ShouldBeEqual(expected);
    }

}