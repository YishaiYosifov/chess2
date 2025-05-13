using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Chess2.Api.TestInfrastructure;

public record ApiClient(IChess2Api Api, HttpClient Client, CookieContainer CookieContainer);
