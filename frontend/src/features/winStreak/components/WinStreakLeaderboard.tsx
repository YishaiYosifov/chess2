"use client";

import LeaderboardMinimalProfileView from "@/features/profile/components/LeaderboardMinimalProfileView";
import Card from "@/components/ui/Card";
import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";
import {
    getWinStreakLeaderboard,
    PagedResultOfWinStreakDto,
    WinStreak,
} from "@/lib/apiClient";
import Link from "next/link";
import constants from "@/lib/constants";
import WinStreakLeaderboardCountdown from "./WinStreakLeaderboardCountdown";

const WinStreakLeaderboard = ({
    initialLeaderboard,
}: {
    initialLeaderboard: PagedResultOfWinStreakDto;
}) => {
    return (
        <Card className="w-full">
            <WinStreakLeaderboardCountdown />

            {initialLeaderboard.totalCount === 0 && (
                <p className="text-error text-center text-2xl">
                    No Players Yet
                </p>
            )}

            <div className="grid grid-cols-[max-content_1fr] gap-3">
                <PaginatedItemsRenderer
                    fetchItems={getWinStreakLeaderboard}
                    initialPaged={initialLeaderboard}
                >
                    {({ items, page, pageSize }) =>
                        items.map((streak, index) => (
                            <WinStreakLeaderboardItem
                                streak={streak}
                                page={page}
                                pageSize={pageSize}
                                index={index}
                                key={streak.profile.userId}
                            />
                        ))
                    }
                </PaginatedItemsRenderer>
            </div>
        </Card>
    );
};
export default WinStreakLeaderboard;

const WinStreakLeaderboardItem = ({
    streak,
    page,
    pageSize,
    index,
}: {
    streak: WinStreak;
    page: number;
    pageSize: number;
    index: number;
}) => {
    return (
        <LeaderboardMinimalProfileView
            profile={streak.profile}
            page={page}
            pageSize={pageSize}
            index={index}
            key={streak.profile.userId}
        >
            <div
                className="grid flex-1 grid-cols-[minmax(20px,1fr)_auto] gap-3"
                data-testid="winStreakLeaderboardItem"
            >
                <div className="overflow-x-auto">
                    <div className="ml-auto flex w-max gap-2">
                        {streak.highestStreakGameTokens.map((token, i) => (
                            <Link
                                key={token}
                                href={`${constants.PATHS.GAME}/${token}`}
                                className="bg-primary rounded-md p-3"
                            >
                                Game #{i + 1}
                            </Link>
                        ))}
                    </div>
                </div>
                <p className="ml-auto flex items-center">
                    {streak.highestStreakGameTokens.length} Win Streak
                </p>
            </div>
        </LeaderboardMinimalProfileView>
    );
};
