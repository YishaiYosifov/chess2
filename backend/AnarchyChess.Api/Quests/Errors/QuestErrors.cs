using AnarchyChess.Api.ErrorHandling.Infrastructure;
using ErrorOr;

namespace AnarchyChess.Api.Quests.Errors;

public static class QuestErrors
{
    public static Error CanotReplace =>
        Error.Forbidden(ErrorCodes.QuestCannotReplace, "Cannot replace quest");

    public static Error NoRewardToCollect =>
        Error.NotFound(ErrorCodes.QuestNoRewardToCollect, "No reward to collect");
}
