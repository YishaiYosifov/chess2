"use client";

import {
    getGameResults,
    PagedResultOfGameSummaryDto,
    User,
} from "@/lib/apiClient";
import GamesTable from "./GamesTable";
import { ButtonHTMLAttributes, JSX, useState } from "react";
import { twMerge } from "tailwind-merge";

const GameHistory = ({
    initialGameResults,
    profileViewpoint,
}: {
    initialGameResults: PagedResultOfGameSummaryDto;
    profileViewpoint: User;
}) => {
    const [gameResults, setPagedGames] = useState(initialGameResults);
    const { items: games, totalPages, page: currentPage } = gameResults;

    const [isFetching, setIsFetching] = useState(false);

    async function fetchGamesForPage(pageNumber: number) {
        setIsFetching(true);
        try {
            const { data: games, error } = await getGameResults({
                path: { userId: profileViewpoint.userId },
                query: { Page: pageNumber, PageSize: 10 },
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

    function renderPageButtons(): JSX.Element[] | null {
        const maxDisplayPages = 3;
        const pages: JSX.Element[] = [];

        let startPageIndex: number;
        if (totalPages - 1 - maxDisplayPages < currentPage)
            startPageIndex = totalPages - maxDisplayPages;
        else startPageIndex = currentPage;
        startPageIndex = Math.max(0, startPageIndex);

        for (
            let i = 0;
            i < maxDisplayPages && startPageIndex + i < totalPages;
            i++
        ) {
            const page = startPageIndex + i;
            pages.push(
                <PaginationButton
                    onClick={() => fetchGamesForPage(page)}
                    key={i}
                    disabled={page === currentPage || isFetching}
                    className={page === currentPage ? "bg-neutral-500" : ""}
                >
                    {page + 1}
                </PaginationButton>,
            );
        }

        if (startPageIndex + maxDisplayPages < totalPages)
            pages.push(<span key="ellipsis">...</span>);

        return pages;
    }

    return (
        <div className="flex w-full flex-col gap-3">
            <GamesTable profileViewpoint={profileViewpoint} games={games} />

            {totalPages > 1 && (
                <div className="flex items-end justify-end gap-3">
                    {currentPage !== 0 && (
                        <>
                            <PaginationButton
                                onClick={() => fetchGamesForPage(0)}
                                disabled={isFetching}
                            >
                                First
                            </PaginationButton>
                            <PaginationButton
                                onClick={() =>
                                    fetchGamesForPage(currentPage - 1)
                                }
                                disabled={isFetching}
                            >
                                ‹
                            </PaginationButton>
                        </>
                    )}

                    {renderPageButtons()}

                    {currentPage < totalPages - 1 && (
                        <>
                            <PaginationButton
                                onClick={() =>
                                    fetchGamesForPage(currentPage + 1)
                                }
                                disabled={isFetching}
                            >
                                ›
                            </PaginationButton>
                            <PaginationButton
                                onClick={() =>
                                    fetchGamesForPage(totalPages - 1)
                                }
                                disabled={isFetching}
                            >
                                Last
                            </PaginationButton>
                        </>
                    )}
                </div>
            )}
        </div>
    );
};
export default GameHistory;

const PaginationButton = ({
    className,
    children,
    ...props
}: ButtonHTMLAttributes<HTMLButtonElement>) => {
    return (
        <button
            className={twMerge(
                `min-w-8 cursor-pointer rounded bg-neutral-800/50 p-1 disabled:cursor-not-allowed
                disabled:brightness-75`,
                className,
            )}
            {...props}
        >
            {children}
        </button>
    );
};
