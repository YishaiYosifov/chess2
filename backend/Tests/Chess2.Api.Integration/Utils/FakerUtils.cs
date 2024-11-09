using Bogus;
using Chess2.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Integration.Utils;

public static class FakerUtils
{
    public static async Task<T> StoreFaker<T>(DbContext dbContext, Faker<T> entity) where T : class
    {
        var generated = entity.Generate();
        await dbContext.AddAsync(generated);
        await dbContext.SaveChangesAsync();
        return generated;
    }
}
