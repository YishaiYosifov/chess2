"use client";

import {
    getGameResults,
    PagedResultOfGameSummaryDto,
    PublicUser,
} from "@/lib/apiClient";
import GamesTable from "./GamesTable";
import PaginatedItemsRenderer from "@/features/pagination/components/PaginatedItemsRenderer";

const GameHistory = ({
    initialGameResults,
    profileViewpoint,
}: {
    initialGameResults: PagedResultOfGameSummaryDto;
    profileViewpoint: PublicUser;
}) => {
    return (
        <div className="flex w-full flex-col gap-3">
            <PaginatedItemsRenderer
                initialPaged={initialGameResults}
                fetchItems={({ query }) =>
                    getGameResults({
                        path: { userId: profileViewpoint.userId },
                        query,
                    })
                }
            >
                {({ items }) => (
                    <GamesTable
                        profileViewpoint={profileViewpoint}
                        games={items}
                    />
                )}
            </PaginatedItemsRenderer>
        </div>
    );
};
export default GameHistory;
