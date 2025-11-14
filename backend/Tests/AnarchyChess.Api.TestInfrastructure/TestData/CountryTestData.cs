namespace AnarchyChess.Api.TestInfrastructure.TestData;

public class CountryCodeTestData : TheoryData<string, bool>
{
    public CountryCodeTestData()
    {
        Add("XZ", false);
        Add("USA", false);
        Add("IL", true);
        Add("XX", true);
        Add("GB-WLS", true);
        Add("GB-NIR", true);
    }
}
