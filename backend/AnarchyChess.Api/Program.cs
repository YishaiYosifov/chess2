using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AnarchyChess.Api.ArchivedGames.Repositories;
using AnarchyChess.Api.ArchivedGames.Services;
using AnarchyChess.Api.Auth.Errors;
using AnarchyChess.Api.Auth.Repositories;
using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Auth.Services.OAuthAuthenticators;
using AnarchyChess.Api.Challenges.Services;
using AnarchyChess.Api.Challenges.SignalR;
using AnarchyChess.Api.ErrorHandling.Extensions;
using AnarchyChess.Api.ErrorHandling.Infrastructure;
using AnarchyChess.Api.ErrorHandling.OpenApi;
using AnarchyChess.Api.Game.Repositories;
using AnarchyChess.Api.Game.SanNotation;
using AnarchyChess.Api.Game.SanNotation.Notators;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.Game.SignalR;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.ForeverRules;
using AnarchyChess.Api.GameLogic.PieceDefinitions;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.GameSnapshot.Services;
using AnarchyChess.Api.GameSnapshot.Validators;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Infrastructure.Extensions;
using AnarchyChess.Api.Infrastructure.OpenAPI;
using AnarchyChess.Api.Infrastructure.Sharding;
using AnarchyChess.Api.Lobby.Grains;
using AnarchyChess.Api.Lobby.Services;
using AnarchyChess.Api.Lobby.SignalR;
using AnarchyChess.Api.Matchmaking.Services;
using AnarchyChess.Api.Pagination.Models;
using AnarchyChess.Api.Pagination.Validators;
using AnarchyChess.Api.Preferences.Repositories;
using AnarchyChess.Api.Preferences.Services;
using AnarchyChess.Api.Profile.DTOs;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Profile.Services;
using AnarchyChess.Api.Profile.Validators;
using AnarchyChess.Api.QuestLogic.QuestDefinitions;
using AnarchyChess.Api.Quests.Grains;
using AnarchyChess.Api.Quests.Repositories;
using AnarchyChess.Api.Quests.Services;
using AnarchyChess.Api.Shared.Models;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.Api.Social.Repository;
using AnarchyChess.Api.Social.Services;
using AnarchyChess.Api.Streaks.Repositories;
using AnarchyChess.Api.Streaks.Services;
using AnarchyChess.Api.UserRating.Repositories;
using AnarchyChess.Api.UserRating.Services;
using ErrorOr;
using FluentStorage;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using NSwag.Generation.Processors;
using Orleans.Configuration;
using Orleans.Serialization.Serializers;
using Orleans.Storage;
using Scalar.AspNetCore;
using Serilog;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Async(a => a.Console())
    .WriteTo.Async(a => a.File("anarchychess.log"))
    .CreateLogger();
builder.Services.AddSerilog();

var appSettingsSection = builder.Configuration.GetSection(nameof(AppSettings));
builder.Services.Configure<AppSettings>(appSettingsSection);
var appSettings =
    builder.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>()
    ?? throw new InvalidOperationException("AppSettings missing");

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    })
    .AddMvcOptions(options =>
    {
        options.Conventions.Add(new UnauthorizedResponseConvention());
        options.Filters.Add<ReformatValidationProblemAttribute>();
    });

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(options =>
{
    options.SchemaSettings.SchemaNameGenerator = new OpenAPIDisplayNameSchemaNameGenerator();
    options.SchemaSettings.SchemaProcessors.Add(new MarkAsRequiredIfNonNullableSchemaProcessor());
});
builder.Services.AddSingleton<IDocumentProcessor, ErrorCodesDocumentProcessor>();
builder.Services.AddSingleton<IOperationProcessor, MethodNameOperationIdProcessor>();

ValidatorOptions.Global.PropertyNameResolver = CamelCasePropertyNameResolver.ResolvePropertyName;

const string AllowCorsOriginName = "AllowCorsOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        AllowCorsOriginName,
        policy =>
            policy
                .WithOrigins(appSettings.CorsOrigins)
                .AllowCredentials()
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

builder.Services.AddSignalR().AddStackExchangeRedis();

#region Database
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.UseNpgsql(appSettings.DatabaseConnString).UseSnakeCaseNamingConvention();
});
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(appSettings.RedisConnString)
);

StorageFactory.Modules.UseAzureBlobStorage();
builder.Services.AddSingleton(
    StorageFactory
        .Blobs.FromConnectionString(appSettings.BlobStorageConnString)
        .WithGzipCompression()
);

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
#endregion

#region Authentication
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AuthPolicies.RefreshAccess,
        policy => policy.RequireClaim("type", "refresh").AddAuthenticationSchemes("RefreshBearer")
    );

    options.AddPolicy(
        AuthPolicies.AuthedUser,
        policy =>
            policy
                .RequireAssertion(context =>
                {
                    var isAccess = context.User.HasClaim("type", "access");
                    var isAnonymous = context.User.HasClaim(ClaimTypes.Anonymous, "1");
                    return isAccess && !isAnonymous;
                })
                .AddAuthenticationSchemes("AccessBearer")
    );

    options.AddPolicy(
        AuthPolicies.ActiveSession,
        policy => policy.RequireClaim("type", "access").AddAuthenticationSchemes("AccessBearer")
    );

    options.DefaultPolicy = options.GetPolicy(AuthPolicies.AuthedUser)!;
});
builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            options.DefaultChallengeScheme =
            options.DefaultForbidScheme =
            options.DefaultSignOutScheme =
            options.DefaultSignInScheme =
            options.DefaultScheme =
                JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(
        "AccessBearer",
        options => ConfigureJwtBearerCookie(options, appSettings.Jwt.AccessTokenCookieName)
    )
    .AddJwtBearer(
        "RefreshBearer",
        options => ConfigureJwtBearerCookie(options, appSettings.Jwt.RefreshTokenCookieName)
    );

void ConfigureJwtBearerCookie(JwtBearerOptions options, string cookieName)
{
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.TokenValidationParameters = new()
    {
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(appSettings.Jwt.SecretKey)
        ),
        ValidIssuer = appSettings.Jwt.Issuer,
        ValidAudience = appSettings.Jwt.Audience,
        ClockSkew = TimeSpan.Zero,
    };

    options.Events = new()
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.TryGetValue(cookieName, out var token))
                ctx.Token = token;
            else if (ctx.Request.Headers.TryGetValue("Authorization", out var headerToken))
                ctx.Token = headerToken;

            return Task.CompletedTask;
        },

        // set a custom unauthorized response
        OnChallenge = ctx =>
        {
            ctx.HandleResponse();
            Error error = ctx.Request.Cookies.ContainsKey(cookieName)
                ? AuthErrors.TokenInvalid
                : AuthErrors.TokenMissing;
            return error
                .ToActionResult()
                .ExecuteResultAsync(new() { HttpContext = ctx.HttpContext });
        },
    };
}

builder
    .Services.AddOpenIddict()
    .AddClient(options =>
    {
        options.DisableTokenStorage();
        options.AllowAuthorizationCodeFlow();

        options
            .UseWebProviders()
            .AddGoogle(options =>
            {
                var clientId =
                    builder.Configuration["Authentication:Google:ClientId"]
                    ?? throw new KeyNotFoundException("Google OAuth Client Id");
                var clientSecret =
                    builder.Configuration["Authentication:Google:ClientSecret"]
                    ?? throw new KeyNotFoundException("Google OAuth Client Secret");

                options.SetClientId(clientId);
                options.SetClientSecret(clientSecret);
                options.AddScopes("email");
                options.SetRedirectUri("api/oauth/google/callback");
            })
            .AddDiscord(options =>
            {
                var clientId =
                    builder.Configuration["Authentication:Discord:ClientId"]
                    ?? throw new KeyNotFoundException("Discord OAuth Client Id");
                var clientSecret =
                    builder.Configuration["Authentication:Discord:ClientSecret"]
                    ?? throw new KeyNotFoundException("Discord OAuth Client Secret");

                options.SetClientId(clientId);
                options.SetClientSecret(clientSecret);
                options.SetRedirectUri("api/oauth/discord/callback");
            });

        options
            .UseAspNetCore()
            .EnableRedirectionEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
        options.AddDevelopmentEncryptionCertificate().AddDevelopmentSigningCertificate();
    });

builder
    .Services.AddIdentityCore<AuthedUser>(options =>
    {
        options.Password = new()
        {
            RequireDigit = false,
            RequiredLength = 8,
            RequireLowercase = false,
            RequireUppercase = false,
            RequireNonAlphanumeric = false,
        };
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddApiEndpoints();

builder.Services.AddScoped<IOAuthService, OAuthService>();
builder.Services.AddScoped<IOAuthProviderNameNormalizer, OAuthProviderNameNormalizer>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenProvider, TokenProvider>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddSingleton<IAuthCookieSetter, AuthCookieSetter>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

builder.Services.AddScoped<IOAuthAuthenticator, GoogleOAuthAuthenticator>();
builder.Services.AddScoped<IOAuthAuthenticator, DiscordOAuthAuthenticator>();

builder.Services.AddScoped<IUsernameGenerator, UsernameGenerator>();
builder.Services.AddSingleton<IUsernameWordsProvider, UsernameWordsProvider>();
#endregion


builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder.UseLocalhostClustering();
    siloBuilder.AddMemoryStreams(Streaming.StreamProvider).AddMemoryGrainStorage("PubSubStore");

    siloBuilder.Configure(
        (GrainCollectionOptions options) =>
        {
            options.ClassSpecificCollectionAge[typeof(PlayerSessionGrain).FullName!] =
                TimeSpan.FromMinutes(5);
        }
    );

    siloBuilder.ConfigureServices(services =>
    {
        services.AddSingleton<IGrainStorageSerializer, OrleansGrainStorageSerializer>();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ISpecializableCodec, GeneratedArrayExpressionCodec>()
        );
    });
    siloBuilder.AddAdoNetGrainStorageAsDefault(options =>
    {
        options.ConnectionString = appSettings.DatabaseConnString;
        options.Invariant = "Npgsql";
    });

    siloBuilder.UseAdoNetReminderService(options =>
    {
        options.ConnectionString = appSettings.DatabaseConnString;
        options.Invariant = "Npgsql";
    });
    siloBuilder.AddStartupTask(
        async (services, token) =>
        {
            var grainFactory = services.GetRequiredService<IGrainFactory>();
            var grain = grainFactory.GetGrain<IQuestSeasonResetterGrain>(0);
            await grain.InitializeAsync();

            for (var i = 0; i < appSettings.Lobby.OpenSeekShardCount; i++)
            {
                await grainFactory.GetGrain<IOpenSeekGrain>(i).InitializeAsync();
            }
        }
    );
});

#region Matchmaking
builder.Services.AddSingleton<ILobbyNotifier, LobbyNotifier>();
builder.Services.AddSingleton<IOpenSeekNotifier, OpenSeekNotifier>();
builder.Services.AddScoped<ISeekerCreator, SeekerCreator>();

builder.Services.AddSingleton<IValidator<TimeControlSettings>, TimeControlSettingsValidator>();
#endregion

#region Game
builder.Services.AddScoped<IGameStarter, GameStarter>();
builder.Services.AddScoped<IGameFinalizer, GameFinalizer>();
builder.Services.AddScoped<IGameArchiveService, GameArchiveService>();
builder.Services.AddScoped<IGameArchiveRepository, GameArchiveRepository>();
builder.Services.AddSingleton<IGameResultDescriber, GameResultDescriber>();
builder.Services.AddSingleton<IGameNotifier, GameNotifier>();
builder.Services.AddSingleton<IRematchNotifier, RematchNotifier>();

builder.Services.AddTransient<IGameCore, GameCore>();
builder.Services.AddTransient<IGameClock, GameClock>();
builder.Services.AddTransient<IDrawEvaulator, DrawEvaulator>();
builder.Services.AddSingleton<ISanCalculator, SanCalculator>();
builder.Services.AddSingleton<IFenCalculator, FenCalculator>();
builder.Services.AddSingleton<IPieceToLetter, PieceToLetter>();
builder.Services.AddSingleton<ITimeControlTranslator, TimeControlTranslator>();
builder.Services.AddSingleton<ILegalMoveCalculator, LegalMoveCalculator>();
builder.Services.AddSingleton<IMoveEncoder, MoveEncoder>();

builder.Services.AddSingleton<ISanNotator, RegularNotator>();
builder.Services.AddSingleton<ISanNotator, KingsideCastleNotator>();
builder.Services.AddSingleton<ISanNotator, QueensideCastleNotator>();
builder.Services.AddSingleton<ISanNotator, VerticalCastleNotator>();
builder.Services.AddSingleton<ISanNotator, IlVaticanoNotator>();
builder.Services.AddSingleton<ISanNotator, BetaDecayNotator>();
builder.Services.AddSingleton<ISanNotator, OmnipotentPawnNotator>();

builder.Services.AddSingleton<IPieceDefinition, KingDefinition>();
builder.Services.AddSingleton<IPieceDefinition, QueenDefinition>();
builder.Services.AddSingleton<IPieceDefinition, PawnDefinition>();
builder.Services.AddSingleton<IPieceDefinition, RookDefinition>();
builder.Services.AddSingleton<IPieceDefinition, BishopDefinition>();
builder.Services.AddSingleton<IPieceDefinition, HorseyDefinition>();
builder.Services.AddSingleton<IPieceDefinition, KnookDefinition>();
builder.Services.AddSingleton<IPieceDefinition, UnderagePawnDefinition>();
builder.Services.AddSingleton<IPieceDefinition, SterilePawnDefinition>();
builder.Services.AddSingleton<IPieceDefinition, AntiqueenDefinition>();
builder.Services.AddSingleton<IPieceDefinition, TraitorRookDefinition>();
builder.Services.AddSingleton<IPieceDefinition, CheckerDefinition>();

builder.Services.AddSingleton<IForeveRule, OmnipotentPawnRule>();
#endregion

#region Rating
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<ICurrentRatingRepository, CurrentRatingRepository>();
builder.Services.AddScoped<IRatingArchiveRepository, RatingArchiveRepository>();
#endregion

#region Game Chat
builder.Services.AddSingleton<IGameChatNotifier, GameChatNotifier>();
builder.Services.AddTransient<IChatRateLimiter, ChatRateLimiter>();
builder.Services.AddScoped<IChatMessageLogger, ChatMessageLogger>();
builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
#endregion

#region Profile
builder.Services.AddScoped<IProfileSettings, ProfileSettings>();
builder.Services.AddScoped<IProfilePictureProvider, ProfilePictureProvider>();
builder.Services.AddSingleton<IValidator<ProfileEditRequest>, ProfileEditValidator>();
builder.Services.AddSingleton<IValidator<UsernameEditRequest>, UsernameEditValidator>();
#endregion

#region Social
builder.Services.AddScoped<IStarRepository, StarRepository>();
builder.Services.AddScoped<IStarService, StarService>();
builder.Services.AddScoped<IBlockRepository, BlockRepository>();
builder.Services.AddScoped<IBlockService, BlockService>();
#endregion

#region Preferences
builder.Services.AddScoped<IPreferenceRepository, PreferenceRepository>();
builder.Services.AddScoped<IPreferenceService, PreferenceService>();
builder.Services.AddScoped<IInteractionLevelGate, InteractionLevelGate>();
#endregion

#region Quests
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddSingleton<IRandomQuestProvider, RandomQuestProvider>();

builder.Services.AddSingleton<IQuestDefinition, WinInQuest>();
builder.Services.AddSingleton<IQuestDefinition, NoCaptureInFirstMovesQuest>();
builder.Services.AddSingleton<IQuestDefinition, CastleCaptureQuest>();
builder.Services.AddSingleton<IQuestDefinition, NoKingMoveQuest>();
builder.Services.AddSingleton<IQuestDefinition, NoQueenMoveQuest>();
builder.Services.AddSingleton<IQuestDefinition, LongPassantCaptureQuest>();
builder.Services.AddSingleton<IQuestDefinition, MinimumGameLengthQuest>();
builder.Services.AddSingleton<IQuestDefinition, CaptureAcrossGamesQuest>();
builder.Services.AddSingleton<IQuestDefinition, MovesAcrossGamesQuest>();
builder.Services.AddSingleton<IQuestDefinition, CappedCapturesQuest>();
builder.Services.AddSingleton<IQuestDefinition, PromoteToAllQuest>();
builder.Services.AddSingleton<IQuestDefinition, PawnPromotionsAcrossGamesQuest>();
builder.Services.AddSingleton<IQuestDefinition, CheckerHopQuest>();
builder.Services.AddSingleton<IQuestDefinition, PerformIlVaticanoQuest>();
builder.Services.AddSingleton<IQuestDefinition, WinGameWith2KingsQuest>();
#endregion

#region Challenges
builder.Services.AddSingleton<IChallengeNotifier, ChallengeNotifier>();
builder.Services.AddScoped<IChallengeRequestCreator, ChallengeRequestCreator>();
#endregion

#region Streaks
builder.Services.AddScoped<IWinStreakRepository, WinStreakRepository>();
builder.Services.AddScoped<IWinStreakService, WinStreakService>();
#endregion

builder.Services.AddSingleton<IShardRouter, ShardRouter>();
builder.Services.AddSingleton<IRandomCodeGenerator, RandomCodeGenerator>();
builder.Services.AddSingleton<IRandomProvider, RandomProvider>();
builder.Services.AddTransient<IStopwatchProvider, StopwatchProvider>();
builder.Services.AddSingleton<IValidator<PaginationQuery>, PaginationValidator>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi(options =>
    {
        options.Path = "/openapi/v1.json";
    });
    app.MapScalarApiReference();
    app.ApplyMigrations();
}

app.UseCors(AllowCorsOriginName);

app.UseStatusCodePagesWithReExecute("/error");
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapControllers();

app.MapHub<ChallengeHub>("/api/hub/challenge");
app.MapHub<OpenSeekHub>("/api/hub/openseek");
app.MapHub<LobbyHub>("/api/hub/lobby");
app.MapHub<GameHub>("/api/hub/game");

app.UseResponseCompression();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

// expose the program for WebApplicationFactory
public partial class Program;
