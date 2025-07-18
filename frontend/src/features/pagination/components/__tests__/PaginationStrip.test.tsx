import React from "react";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import PaginationStrip from "../PaginationStrip";

describe("PaginationStrip", () => {
    const fetchItemsForPageMock = vi.fn().mockResolvedValue(undefined);

    const setup = (
        props?: Partial<React.ComponentProps<typeof PaginationStrip>>,
    ) =>
        render(
            <PaginationStrip
                currentPage={0}
                totalPages={5}
                isFetching={false}
                fetchItemsForPage={fetchItemsForPageMock}
                {...props}
            />,
        );

    it.each([0, 1])(
        "should render nothing when totalPages <= 1",
        (totalPages) => {
            const { container } = setup({ totalPages });
            expect(container.firstChild).toBeNull();
        },
    );

    it("should not show First/Prev on the first page and shows Next/Last", () => {
        setup({ currentPage: 0 });

        expect(screen.queryByTestId("paginationFirst")).not.toBeInTheDocument();
        expect(screen.queryByTestId("paginationPrev")).not.toBeInTheDocument();

        expect(screen.getByTestId("paginationNext")).toBeInTheDocument();
        expect(screen.getByTestId("paginationLast")).toBeInTheDocument();
    });

    it("should show First/Prev when not on the first page", () => {
        setup({ currentPage: 2 });

        expect(screen.getByTestId("paginationFirst")).toBeInTheDocument();
        expect(screen.getByTestId("paginationPrev")).toBeInTheDocument();
    });

    it("should renders at most 3 numbered buttons + an ellipsis when pages remain", () => {
        setup({ currentPage: 0 });

        expect(screen.getByTestId("paginationPage0")).toHaveTextContent("1");
        expect(screen.getByTestId("paginationPage1")).toHaveTextContent("2");
        expect(screen.getByTestId("paginationPage2")).toHaveTextContent("3");

        // pages 4-5 still exist
        expect(screen.getByTestId("paginationEllipsis")).toBeInTheDocument();
    });

    it("should shifts the page window near the end (page 3 of 5)", () => {
        setup({ currentPage: 3 });

        expect(screen.getByTestId("paginationPage2")).toHaveTextContent("3");
        expect(screen.getByTestId("paginationPage3")).toHaveTextContent("4");
        expect(screen.getByTestId("paginationPage4")).toHaveTextContent("5");

        expect(
            screen.queryByTestId("paginationEllipsis"),
        ).not.toBeInTheDocument();
    });

    it("should disable all buttons when fetching", () => {
        setup({ currentPage: 1, isFetching: true });

        expect(screen.getByTestId("paginationFirst")).toBeDisabled();
        expect(screen.getByTestId("paginationPrev")).toBeDisabled();
        expect(screen.getByTestId("paginationNext")).toBeDisabled();
        expect(screen.getByTestId("paginationLast")).toBeDisabled();

        expect(screen.getByTestId("paginationPage1")).toBeDisabled();
        expect(screen.getByTestId("paginationPage2")).toBeDisabled();
        expect(screen.getByTestId("paginationPage3")).toBeDisabled();
    });

    it("should call fetchItemsForPage with the correct page on click", async () => {
        setup({ currentPage: 1 });

        await userEvent.click(screen.getByTestId("paginationPage2"));
        expect(fetchItemsForPageMock).toHaveBeenCalledWith(2);

        await userEvent.click(screen.getByTestId("paginationPrev"));
        expect(fetchItemsForPageMock).toHaveBeenCalledWith(0);

        await userEvent.click(screen.getByTestId("paginationNext"));
        expect(fetchItemsForPageMock).toHaveBeenCalledWith(2);

        await userEvent.click(screen.getByTestId("paginationFirst"));
        expect(fetchItemsForPageMock).toHaveBeenCalledWith(0);

        await userEvent.click(screen.getByTestId("paginationLast"));
        expect(fetchItemsForPageMock).toHaveBeenCalledWith(4);

        expect(fetchItemsForPageMock).toHaveBeenCalledTimes(5);
    });
});
