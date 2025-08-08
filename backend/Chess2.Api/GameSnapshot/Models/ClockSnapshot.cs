using System.ComponentModel;

namespace Chess2.Api.GameSnapshot.Models;

[DisplayName("Clocks")]
[GenerateSerializer]
[Alias("Chess2.Api.GameSnapshot.Models.ClockSnapshot")]
public record ClockSnapshot(double WhiteClock, double BlackClock, double? LastUpdated);
