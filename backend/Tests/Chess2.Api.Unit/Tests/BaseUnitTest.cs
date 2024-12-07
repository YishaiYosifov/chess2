using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Chess2.Api.Models;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Unit.Tests;

public class BaseUnitTest
{
    protected Fixture Fixture = new();

    public BaseUnitTest()
    {
        Fixture.Customize(new AutoNSubstituteCustomization());
        AddAppSettings(Fixture);
    }

    private static void AddAppSettings(Fixture fixture)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var appSettings = configuration.GetSection("AppSettings").Get<AppSettings>()!;
        fixture.Register(() => Options.Create(appSettings));
    }
}
