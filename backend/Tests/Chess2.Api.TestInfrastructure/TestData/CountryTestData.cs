namespace Chess2.Api.TestInfrastructure.TestData;

public class CountryCodeTestData : TheoryData<string?, bool>
{
    public CountryCodeTestData()
    {
        Add("XZ", false);
        Add("USA", false);
        Add("IL", true);
        Add(null, true);
    }
}
