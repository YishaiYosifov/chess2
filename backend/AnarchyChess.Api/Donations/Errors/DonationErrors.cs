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
    public static Error InvalidWebhookVerificationCode =>
        Error.Unauthorized(
            ErrorCodes.DonationWebhookInvalidVerificationCode,
            "The provided webhook verification code is invalid"
        );
}
