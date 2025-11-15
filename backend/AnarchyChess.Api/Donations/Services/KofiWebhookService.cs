using AnarchyChess.Api.Donations.Entities;
using AnarchyChess.Api.Donations.Errors;
using AnarchyChess.Api.Donations.Models;
using AnarchyChess.Api.Donations.Repositories;
using AnarchyChess.Api.Shared.Services;
using ErrorOr;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnarchyChess.Api.Donations.Services;

public interface IKofiWebhookService
{
    Task<ErrorOr<Updated>> ReceiveWebhookAsync(string data, CancellationToken token = default);
}

public class KofiWebhookService(
    ILogger<KofiWebhookService> logger,
    IDonationRepository donationRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration
) : IKofiWebhookService
{
    private static readonly JsonSerializerOptions _kofiJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly ILogger<KofiWebhookService> _logger = logger;
    private readonly IDonationRepository _donationRepository = donationRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IConfiguration _configuration = configuration;

    public async Task<ErrorOr<Updated>> ReceiveWebhookAsync(
        string data,
        CancellationToken token = default
    )
    {
        var kofiDonationResult = ParseWebhookData(data);
        if (kofiDonationResult.IsError)
            return kofiDonationResult.Errors;
        var kofiDonation = kofiDonationResult.Value;

        if (!kofiDonation.IsPublic)
        {
            _logger.LogInformation(
                "{Name} donated {Amount} anonymously, thank you :D",
                kofiDonation.FromName,
                kofiDonation.Amount
            );
            return Result.Updated;
        }
        await RecordDonationAsync(kofiDonation, token);
        await _unitOfWork.CompleteAsync(token);

        _logger.LogInformation(
            "{Name} donated {Amount}, thank you :D",
            kofiDonation.FromName,
            kofiDonation.Amount
        );
        return Result.Updated;
    }

    private ErrorOr<KofiDonation> ParseWebhookData(string data)
    {
        KofiDonation? kofiDonation;
        try
        {
            kofiDonation = JsonSerializer.Deserialize<KofiDonation>(data, _kofiJsonOptions);
        }
        catch (JsonException)
        {
            _logger.LogWarning("Invalid Kofi webhook JSON: {Data}", data);
            return DonationErrors.InvalidWebhookJson;
        }
        if (kofiDonation is null)
        {
            _logger.LogWarning("Invalid Kofi webhook JSON: {Data}", data);
            return DonationErrors.InvalidWebhookJson;
        }
        if (kofiDonation.VerificationCode != _configuration["Kofi:WebhookVerificationCode"])
        {
            _logger.LogWarning(
                "Invalid Kofi webhook verification code: {VerificationCode}",
                kofiDonation.VerificationCode
            );
            return DonationErrors.InvalidWebhookJson;
        }
        return kofiDonation;
    }

    private async Task RecordDonationAsync(
        KofiDonation kofiDonation,
        CancellationToken token = default
    )
    {
        decimal decimalAmount = decimal.Parse(kofiDonation.Amount);
        var existingDonation = await _donationRepository.GetByEmailAsync(kofiDonation.Email, token);
        if (existingDonation is not null)
        {
            existingDonation.TotalAmount += decimalAmount;
            existingDonation.Name = kofiDonation.FromName;
        }
        else
        {
            Donation newDonation = new()
            {
                Email = kofiDonation.Email,
                Name = kofiDonation.FromName,
                TotalAmount = decimalAmount,
            };
            await _donationRepository.AddAsync(newDonation, token);
        }
    }
}
