"use client";

import React from "react";

import {
    getQuestLeaderboard,
    PagedResultOfQuestPointsDto,
} from "@/lib/apiClient";

import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";
import Card from "@/components/ui/Card";

import LeaderboardResetCountdown from "./LeaderboardResetCountdown";
import LeaderboardMinimalProfileView from "@/features/profile/components/LeaderboardMinimalProfileView";

const QuestLeaderboard = ({
    initialLeaderboard,
}: {
    initialLeaderboard: PagedResultOfQuestPointsDto;
}) => {
    return (
        <Card className="w-full flex-1 gap-5 p-6">
            <div
                className="flex flex-col flex-wrap items-center justify-center gap-2 sm:flex-row
                    sm:justify-between"
            >
                <h1 className="text-2xl">Quest Leaderboard</h1>

                <LeaderboardResetCountdown />
            </div>

            {initialLeaderboard.totalCount === 0 && (
                <p className="text-error text-center text-2xl">
                    No Players Yet
                </p>
            )}

            <PaginatedItemsRenderer
                fetchItems={getQuestLeaderboard}
                initialPaged={initialLeaderboard}
            >
                {({ items, page, pageSize }) => (
                    <div className="grid grid-cols-[max-content_1fr] gap-3">
                        {items.map((profileQuestPoints, index) => (
                            <LeaderboardMinimalProfileView
                                profile={profileQuestPoints.profile}
                                page={page}
                                pageSize={pageSize}
                                index={index}
                                key={profileQuestPoints.profile.userId}
                            >
                                <p
                                    className="ml-auto flex items-center gap-2"
                                    data-testid={`questLeaderboardPoints-${profileQuestPoints.profile.userId}`}
                                >
                                    {profileQuestPoints.questPoints} points
                                </p>
                            </LeaderboardMinimalProfileView>
                        ))}
                    </div>
                )}
            </PaginatedItemsRenderer>
        </Card>
    );
};
export default QuestLeaderboard;
