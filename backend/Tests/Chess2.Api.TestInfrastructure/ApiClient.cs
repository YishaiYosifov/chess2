using System.Net;

namespace Chess2.Api.TestInfrastructure;

public record ApiClient(IChess2Api Api, HttpClient Client, CookieContainer CookieContainer);
