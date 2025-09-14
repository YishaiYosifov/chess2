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
    myQuestRanking,
}: {
    initialLeaderboard: PagedResultOfQuestPointsDto;
    myQuestRanking?: number;
}) => {
    return (
        <Card className="w-full gap-5 p-6">
            <div>
                <h1 className="text-3xl" data-testid="questLeaderboardTitle">
                    Leaderboard
                </h1>

                {myQuestRanking && (
                    <p
                        className="text-text/70"
                        data-testid="myQuestRankingDisplay"
                    >
                        You are ranked{" "}
                        <span className="text-amber-400">
                            #{myQuestRanking}
                        </span>
                    </p>
                )}
            </div>

            <PaginatedItemsRenderer
                fetchItems={getQuestLeaderboard}
                initialPaged={initialLeaderboard}
            >
                {({ items, page, pageSize }) => (
                    <div className="grid grid-cols-[max-content_1fr] gap-3">
                        {items.map((profileQuestPoints, index) => (
                            <QuestLeaderboardItem
                                profileQuestPoints={profileQuestPoints}
                                index={index}
                                overallPosition={page * pageSize + index + 1}
                                key={profileQuestPoints.profile.userId}
                            />
                        ))}
                    </div>
                )}
            </PaginatedItemsRenderer>
        </Card>
    );
};
export default QuestLeaderboard;

const QuestLeaderboardItem = ({
    index,
    profileQuestPoints,
    overallPosition,
}: {
    index: number;
    profileQuestPoints: UserQuestPoints;
    overallPosition: number;
}) => {
    const podiumColors = ["bg-amber-400", "bg-slate-300", "bg-orange-400"];
    const podiumIcon = ["🥇", "🥈", "🥉"];
    const colorClass =
        overallPosition <= podiumColors.length
            ? podiumColors[overallPosition - 1]
            : "bg-text/70";
    const rankDisplay =
        overallPosition <= podiumIcon.length
            ? podiumIcon[overallPosition - 1]
            : `#${overallPosition}`;

    return (
        <>
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
        </>
    );
};
