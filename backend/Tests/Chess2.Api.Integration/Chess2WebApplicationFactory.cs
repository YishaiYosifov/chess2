using Chess2.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Integration;

public class Chess2WebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // remove the existing database context, use an in-memory one instead
            services.RemoveAll(typeof(DbContextOptions<Chess2DbContext>));
            services.AddDbContextPool<Chess2DbContext>(options =>
                options.UseInMemoryDatabase("TestDB"));
        });
    }
}
