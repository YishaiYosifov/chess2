using System.Net;

namespace AnarchyChess.Api.TestInfrastructure;

public record ApiClient(IAnarchyChessApi Api, HttpClient Client, CookieContainer CookieContainer);
