using Chess2.Api.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.Integration;

public interface IChess2Api
{
    [Post("/auth/register")]
    Task Register([Body] UserLogin userLogin);
}
