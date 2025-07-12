using System.ComponentModel;

namespace Chess2.Api.Game.Models;

[DisplayName("Clocks")]
public record ClockDto(double WhiteClock, double BlackClock, double? LastUpdated);
