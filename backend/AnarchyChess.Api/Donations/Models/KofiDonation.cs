namespace AnarchyChess.Api.Donations.Models;

public record KofiDonation(
    string VerificationCode,
    string Email,
    string FromName,
    bool IsPublic,
    string Amount
);
