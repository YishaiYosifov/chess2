import { useState } from "react";
import PaginationStrip from "./PaginationStrip";

const PaginatedItemsRenderer = <TItem,>({
    page,
    totalPages,
    items,
    paginatedItem,
    fetchItemsForPage,
}: {
    page: number;
    totalPages: number;
    items: TItem[];
    paginatedItem: (item: TItem, index: number) => React.ReactNode;
    fetchItemsForPage: (pageNumber: number) => Promise<void>;
}) => {
    const [isFetching, setIsFetching] = useState(false);

    async function handleFetch(pageNumber: number) {
        setIsFetching(true);
        try {
            await fetchItemsForPage(pageNumber);
        } finally {
            setIsFetching(false);
        }
    }

    return (
        <>
            {items.map((item, i) => paginatedItem(item, i))}
            <PaginationStrip
                currentPage={page}
                totalPages={totalPages}
                isFetching={isFetching}
                fetchItemsForPage={handleFetch}
            />
        </>
    );
};
export default PaginatedItemsRenderer;
