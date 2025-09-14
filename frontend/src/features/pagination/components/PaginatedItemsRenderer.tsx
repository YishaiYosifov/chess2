import { useState } from "react";
import PaginationStrip from "./PaginationStrip";
import { Renderable, renderRenderable } from "@/lib/utils/renderable";
import { PagedResult } from "../lib/types";

interface PaginatedItemsRendererChildren<TItem> extends PagedResult<TItem> {
    incrementTotalCount(): void;
    decrementTotalCount(): void;
}

const PaginatedItemsRenderer = <TItem,>({
    initialPaged,
    children,
    fetchItems,
}: {
    initialPaged: PagedResult<TItem>;
    children: Renderable<PaginatedItemsRendererChildren<TItem>>;
    fetchItems: (args: {
        query: { Page: number; PageSize: number };
    }) => Promise<{ data: PagedResult<TItem> | undefined; error: unknown }>;
}) => {
    const [isFetching, setIsFetching] = useState(false);
    const [pagedResult, setPagedResult] = useState(initialPaged);

    async function handleFetch(pageNumber: number) {
        setIsFetching(true);
        try {
            const { data, error } = await fetchItems({
                query: { Page: pageNumber, PageSize: pagedResult.pageSize },
            });
            if (error || data === undefined) {
                console.error(error);
                return;
            }

            setPagedResult(data);
        } finally {
            setIsFetching(false);
        }
    }

    function incrementTotalCount() {
        setPagedResult((prev) => ({
            ...prev,
            totalCount: prev.totalCount + 1,
        }));
    }
    function decrementTotalCount() {
        setPagedResult((prev) => ({
            ...prev,
            totalCount: prev.totalCount - 1,
        }));
    }

    return (
        <>
            {renderRenderable(children, {
                ...pagedResult,
                incrementTotalCount,
                decrementTotalCount,
            })}
            <PaginationStrip
                currentPage={pagedResult.page}
                totalPages={pagedResult.totalPages}
                isFetching={isFetching}
                fetchItemsForPage={handleFetch}
            />
        </>
    );
};
export default PaginatedItemsRenderer;
