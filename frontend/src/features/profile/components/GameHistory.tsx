"use client";

import { useState } from "react";

import {
    getGameResults,
    PagedResultOfGameSummaryDto,
    User,
} from "@/lib/apiClient";
import PaginationStrip from "@/features/pagination/components/PaginationStrip";
import GamesTable from "./GamesTable";

const GameHistory = ({
    initialGameResults,
    profileViewpoint,
}: {
    initialGameResults: PagedResultOfGameSummaryDto;
    profileViewpoint: User;
}) => {
    const [gameResults, setPagedGames] = useState(initialGameResults);
    const [isFetching, setIsFetching] = useState(false);

    async function fetchGamesForPage(pageNumber: number): Promise<void> {
        try {
            setIsFetching(true);
            const { data: games, error } = await getGameResults({
                path: { userId: profileViewpoint.userId },
                query: {
                    Page: pageNumber,
                    PageSize: gameResults.pageSize,
                },
            });
            if (error || !games) {
                console.error(error);
                return;
            }

            setPagedGames(games);
        } finally {
            setIsFetching(false);
        }
    }

    return (
        <div className="flex w-full flex-col gap-3">
            <GamesTable
                profileViewpoint={profileViewpoint}
                games={gameResults.items}
            />
            <PaginationStrip
                currentPage={gameResults.page}
                totalPages={gameResults.totalPages}
                isFetching={isFetching}
                fetchItemsForPage={fetchGamesForPage}
            />
        </div>
    );
};
export default GameHistory;
