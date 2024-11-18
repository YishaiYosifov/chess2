namespace Chess2.Api.TestInfrastructure.TestData;

public static class CountryTestData
{
    public static IEnumerable<object?[]> CodeValidationData =>
        [
            ["XZ", false],
            ["USA", false],
            ["IL", true],
            [null, true],
        ];
}
