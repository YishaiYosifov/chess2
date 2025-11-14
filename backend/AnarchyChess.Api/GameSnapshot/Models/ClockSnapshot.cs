using System.ComponentModel;

namespace AnarchyChess.Api.GameSnapshot.Models;

[DisplayName("Clocks")]
[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.ClockSnapshot")]
public record ClockSnapshot(double WhiteClock, double BlackClock, double? LastUpdated);
