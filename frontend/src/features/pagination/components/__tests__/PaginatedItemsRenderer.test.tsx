import { render, screen, waitFor } from "@testing-library/react";

import userEvent from "@testing-library/user-event";
import PaginatedItemsRenderer from "../PaginatedItemsRenderer";

describe("PaginatedItemsRenderer", () => {
    const fetchItemsForPage = vi.fn();

    it("should render items using the paginatedItem function", () => {
        const items = ["a", "b", "c"];

        render(
            <PaginatedItemsRenderer
                page={0}
                totalPages={1}
                items={items}
                fetchItemsForPage={fetchItemsForPage}
                paginatedItem={(item, i) => (
                    <div key={i} data-testid={`item-${i}`}>
                        {item}
                    </div>
                )}
            />,
        );

        expect(screen.getByTestId("item-0")).toHaveTextContent("a");
        expect(screen.getByTestId("item-1")).toHaveTextContent("b");
        expect(screen.getByTestId("item-2")).toHaveTextContent("c");
    });

    it("should call fetchItemsForPage when handleFetch is triggered", async () => {
        const user = userEvent.setup();

        render(
            <PaginatedItemsRenderer
                page={0}
                totalPages={2}
                items={["x"]}
                fetchItemsForPage={fetchItemsForPage}
                paginatedItem={(_, i) => <div key={i} />}
            />,
        );

        const nextButton = screen.getByTestId("paginationNext");
        await user.click(nextButton);

        expect(fetchItemsForPage).toHaveBeenCalledWith(1);
    });

    it("should disable pagination buttons while loading", async () => {
        let resolveFetch!: () => void;
        const fetchPromise = new Promise<void>((resolve) => {
            resolveFetch = resolve;
        });
        fetchItemsForPage.mockReturnValue(fetchPromise);

        render(
            <PaginatedItemsRenderer
                page={0}
                totalPages={2}
                items={["y"]}
                fetchItemsForPage={fetchItemsForPage}
                paginatedItem={(_, i) => <div key={i} />}
            />,
        );

        expect(screen.getByTestId("paginationPage1")).not.toBeDisabled();
        await userEvent.click(screen.getByTestId("paginationPage1"));
        expect(screen.getByTestId("paginationPage1")).toBeDisabled();

        resolveFetch();
        await waitFor(() => expect(fetchItemsForPage).toHaveBeenCalled());
        expect(screen.getByTestId("paginationPage1")).not.toBeDisabled();
    });
});
