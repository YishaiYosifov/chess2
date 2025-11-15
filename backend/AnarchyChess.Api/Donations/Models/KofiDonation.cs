namespace AnarchyChess.Api.Donations.Models;

public record KofiDonation(
    string VerificationCode,
    DonationType Type,
    string Email,
    string FromName,
    bool IsPublic,
    string Amount
);
