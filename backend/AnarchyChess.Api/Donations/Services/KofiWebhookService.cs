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
    IUnitOfWork unitOfWork
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

    public async Task<ErrorOr<Updated>> ReceiveWebhookAsync(
        string data,
        CancellationToken token = default
    )
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

        await _unitOfWork.CompleteAsync(token);
        return Result.Updated;
    }
}
