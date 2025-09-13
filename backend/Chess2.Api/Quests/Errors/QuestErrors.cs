using Chess2.Api.Infrastructure.Errors;
using ErrorOr;

namespace Chess2.Api.Quests.Errors;

public static class QuestErrors
{
    public static Error CanotReplace =>
        Error.Forbidden(ErrorCodes.QuestCannotReplace, "Cannot replace quest");

    public static Error NoRewardToCollect =>
        Error.NotFound(ErrorCodes.QuestNoRewardToCollect, "No reward to collect");
}
