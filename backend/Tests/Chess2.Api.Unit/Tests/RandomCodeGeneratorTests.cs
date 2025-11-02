using Chess2.Api.Shared.Services;
using FluentAssertions;

namespace Chess2.Api.Unit.Tests;

public class RandomCodeGeneratorTests : BaseUnitTest
{
    private readonly RandomCodeGenerator _randomCodeGenerator = new();

    [Fact]
    public void Generate_generates_a_string_of_the_correct_length()
    {
        int length = 10;
        string code = _randomCodeGenerator.Generate(length);

        code.Should().NotBeNullOrEmpty();
        code.Length.Should().Be(length);
    }

    [Fact]
    public void Generate_generate_different_codes_for_each_call()
    {
        int length = 16;
        string code1 = _randomCodeGenerator.Generate(length);
        string code2 = _randomCodeGenerator.Generate(length);

        code1.Should().NotBe(code2);
    }
}
