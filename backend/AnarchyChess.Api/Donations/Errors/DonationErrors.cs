using AnarchyChess.Api.Infrastructure.Errors;
using ErrorOr;

namespace AnarchyChess.Api.Donations.Errors;

public static class DonationErrors
{
    public static Error InvalidWebhookJson =>
        Error.Validation(
            ErrorCodes.DonationWebhookInvalidJson,
            "The provided webhook JSON is invalid"
        );
}
