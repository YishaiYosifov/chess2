import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import PaginatedItemsRenderer from "../PaginatedItemsRenderer";
import { PagedResult } from "../../lib/types";

describe("PaginatedItemsRenderer", () => {
    const fetchItems = vi.fn();

    const initialPaged: PagedResult<string> = {
        items: ["a", "b", "c"],
        totalCount: 3,
        page: 0,
        pageSize: 10,
        totalPages: 2,
    };

    it("should render items using the children function", () => {
        render(
            <PaginatedItemsRenderer
                initialPaged={initialPaged}
                fetchItems={fetchItems}
            >
                {({ items }) => (
                    <div>
                        {items.map((item, i) => (
                            <div key={i} data-testid={`item-${i}`}>
                                {item}
                            </div>
                        ))}
                    </div>
                )}
            </PaginatedItemsRenderer>,
        );

        expect(screen.getByTestId("item-0")).toHaveTextContent("a");
        expect(screen.getByTestId("item-1")).toHaveTextContent("b");
        expect(screen.getByTestId("item-2")).toHaveTextContent("c");
    });

    it("should call fetchItems when pagination is triggered", async () => {
        const user = userEvent.setup();
        fetchItems.mockResolvedValue({ data: initialPaged, error: null });

        render(
            <PaginatedItemsRenderer
                initialPaged={initialPaged}
                fetchItems={fetchItems}
            >
                {({ items }) => (
                    <div>
                        {items.map((item, i) => (
                            <div key={i}>{item}</div>
                        ))}
                    </div>
                )}
            </PaginatedItemsRenderer>,
        );

        const nextButton = screen.getByTestId("paginationNext");
        await user.click(nextButton);

        expect(fetchItems).toHaveBeenCalledWith({
            query: {
                Page: initialPaged.page + 1,
                PageSize: initialPaged.pageSize,
            },
        });
    });

    it("should disable pagination buttons while loading", async () => {
        const user = userEvent.setup();

        let resolveFetch!: () => void;
        const fetchPromise = new Promise<{
            data: typeof initialPaged;
            error: unknown;
        }>((resolve) => {
            resolveFetch = () => resolve({ data: initialPaged, error: null });
        });
        fetchItems.mockReturnValue(fetchPromise);

        render(
            <PaginatedItemsRenderer
                initialPaged={initialPaged}
                fetchItems={fetchItems}
            >
                {({ items }) => (
                    <div>
                        {items.map((item, i) => (
                            <div key={i}>{item}</div>
                        ))}
                    </div>
                )}
            </PaginatedItemsRenderer>,
        );

        const page1Button = screen.getByTestId("paginationPage1");
        expect(page1Button).not.toBeDisabled();

        await user.click(page1Button);
        expect(page1Button).toBeDisabled();

        resolveFetch();
        await waitFor(() => expect(fetchItems).toHaveBeenCalled());
        expect(page1Button).not.toBeDisabled();
    });

    it("should increment and decrement totalCount correctly", async () => {
        const user = userEvent.setup();

        render(
            <PaginatedItemsRenderer
                initialPaged={initialPaged}
                fetchItems={fetchItems}
            >
                {({ totalCount, incrementTotalCount, decrementTotalCount }) => (
                    <div>
                        <span data-testid="count">{totalCount}</span>
                        <button onClick={incrementTotalCount}>+</button>
                        <button onClick={decrementTotalCount}>-</button>
                    </div>
                )}
            </PaginatedItemsRenderer>,
        );

        const count = screen.getByTestId("count");
        const incrementBtn = screen.getByText("+");
        const decrementBtn = screen.getByText("-");

        expect(count).toHaveTextContent("3");

        await user.click(incrementBtn);
        expect(count).toHaveTextContent("4");

        await user.click(decrementBtn);
        expect(count).toHaveTextContent("3");
    });
});
