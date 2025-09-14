"use client";

import Card from "@/components/ui/Card";
import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";
import MinimalProfileView from "@/features/profile/components/MinimalProfileView";
import {
    getQuestLeaderboard,
    PagedResultOfQuestPointsDto,
    UserQuestPoints,
} from "@/lib/apiClient";
import clsx from "clsx";
import React from "react";

const QuestLeaderboard = ({
    initialLeaderboard,
}: {
    initialLeaderboard: PagedResultOfQuestPointsDto;
}) => {
    return (
        <Card className="w-full max-w-3xl">
            <h1 className="text-3xl" data-testid="questLeaderboardTitle">
                Leaderboard
            </h1>

            <PaginatedItemsRenderer
                fetchItems={getQuestLeaderboard}
                initialPaged={initialLeaderboard}
            >
                {QuestLeaderboardItemRenderer}
            </PaginatedItemsRenderer>
        </Card>
    );
};
export default QuestLeaderboard;

const QuestLeaderboardItemRenderer = ({
    items,
    page,
    pageSize,
}: {
    items: UserQuestPoints[];
    page: number;
    pageSize: number;
}) => {
    const podiumColors = ["bg-yellow-400", "bg-slate-300", "bg-orange-400"];
    const podiumIcon = ["🥇", "🥈", "🥉"];

    return (
        <div className="grid grid-cols-[max-content_1fr] gap-3">
            {items.map((profileQuestPoints, index) => {
                const overallPosition = page * pageSize + index + 1;
                const colorClass =
                    overallPosition <= podiumColors.length
                        ? podiumColors[overallPosition - 1]
                        : "bg-text/70";
                const rankDisplay =
                    overallPosition <= podiumIcon.length
                        ? podiumIcon[overallPosition - 1]
                        : `#${overallPosition}`;

                return (
                    <React.Fragment key={profileQuestPoints.profile.userId}>
                        <span
                            className={clsx(
                                "flex h-full items-center justify-center rounded-md p-2 text-black",
                                colorClass,
                            )}
                            data-testid={`questLeaderboardRankDisplay-${profileQuestPoints.profile.userId}`}
                        >
                            {rankDisplay}
                        </span>

                        <MinimalProfileView
                            profile={profileQuestPoints.profile}
                            index={index}
                        >
                            <p
                                className="ml-auto flex items-center gap-2"
                                data-testid={`questLeaderboardPoints-${profileQuestPoints.profile.userId}`}
                            >
                                {profileQuestPoints.questPoints} points
                            </p>
                        </MinimalProfileView>
                    </React.Fragment>
                );
            })}
        </div>
    );
};
