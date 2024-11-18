using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.TestInfrastructure.TestData;

public static class CountryCodeTestData
{
    public static IEnumerable<object?[]> CountryValidationData =>
        [
            ["XZ", false],
            ["IL", true],
            [null, true],
        ];
}
