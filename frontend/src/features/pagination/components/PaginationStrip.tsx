import { JSX } from "react";
import PaginationButton from "./PaginationButton";

const PaginationStrip = ({
    currentPage,
    totalPages,
    isFetching,
    fetchItemsForPage,
}: {
    currentPage: number;
    totalPages: number;
    isFetching: boolean;
    fetchItemsForPage(page: number): Promise<void>;
}) => {
    if (totalPages <= 1) return null;

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
                    onClick={() => fetchItemsForPage(page)}
                    disabled={page === currentPage || isFetching}
                    className={page === currentPage ? "bg-neutral-500" : ""}
                    key={i}
                    data-testid={`paginationPage${page}`}
                >
                    {page + 1}
                </PaginationButton>,
            );
        }

        if (startPageIndex + maxDisplayPages < totalPages)
            pages.push(
                <span key="ellipsis" data-testid="paginationEllipsis">
                    ...
                </span>,
            );

        return pages;
    }

    return (
        <div className="flex items-end justify-end gap-3">
            {currentPage !== 0 && (
                <>
                    <PaginationButton
                        onClick={() => fetchItemsForPage(0)}
                        disabled={isFetching}
                        data-testid="paginationFirst"
                    >
                        First
                    </PaginationButton>
                    <PaginationButton
                        onClick={() => fetchItemsForPage(currentPage - 1)}
                        disabled={isFetching}
                        data-testid="paginationPrev"
                    >
                        ‹
                    </PaginationButton>
                </>
            )}

            {renderPageButtons()}

            {currentPage < totalPages - 1 && (
                <>
                    <PaginationButton
                        onClick={() => fetchItemsForPage(currentPage + 1)}
                        disabled={isFetching}
                        data-testid="paginationNext"
                    >
                        ›
                    </PaginationButton>
                    <PaginationButton
                        onClick={() => fetchItemsForPage(totalPages - 1)}
                        disabled={isFetching}
                        data-testid="paginationLast"
                    >
                        Last
                    </PaginationButton>
                </>
            )}
        </div>
    );
};
export default PaginationStrip;
