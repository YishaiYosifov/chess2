using Chess2Backend.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2Backend.Integration;

public interface IChess2Backend
{
    [Post("/auth/register")]
    Task Register([Body] UserLogin userLogin);
}
