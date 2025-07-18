using System.ComponentModel;

namespace Chess2.Api.GameSnapshot.Models;

[DisplayName("Clocks")]
public record ClockSnapshot(double WhiteClock, double BlackClock, double? LastUpdated);
