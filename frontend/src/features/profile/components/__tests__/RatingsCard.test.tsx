import { render, screen } from "@testing-library/react";
import Chart from "react-apexcharts";

import preloadAll from "@/lib/testUtils/dynamicImportMock";

import RatingCard from "../RatingsCard";
import { RatingOverview } from "@/lib/apiClient";
import { createFakeRatingOverview } from "@/lib/testUtils/fakers/ratingOverviewFaker";

vi.mock("react-apexcharts");

describe("RatingsCard", () => {
    const chartMock = vi.mocked(Chart);
    let ratingMock: RatingOverview;

    beforeAll(() => preloadAll());

    beforeEach(() => {
        ratingMock = createFakeRatingOverview();
    });

    it("should render the card with variant, rating data, and chart", () => {
        render(<RatingCard overview={ratingMock} />);

        expect(screen.getByTestId("chart")).toBeInTheDocument();
        expect(screen.queryByTestId("ratingInfoSection")).toBeInTheDocument();
    });

    it("should render correct formatted ratings in chart series", () => {
        const currDate = new Date("2024-11-24T12:00:00").getTime();
        vi.setSystemTime(currDate);

        render(<RatingCard overview={ratingMock} />);

        const callArgs = chartMock.mock.calls[0][0];
        expect(callArgs.series).toEqual([
            {
                name: "Rating",
                data: ratingMock.ratings.map(({ achievedAt, rating }) => ({
                    x: new Date(achievedAt).valueOf(),
                    y: rating,
                })),
            },
        ]);
    });

    it("should display current, highest, lowest ratings correctly", () => {
        render(<RatingCard overview={ratingMock} />);

        expect(screen.getByTestId("currentRating").textContent).toBe(
            ratingMock.current.toString(),
        );
        const maxRating = screen.getByTestId("maxRating");
        const minRating = screen.getByTestId("minRating");
        expect(maxRating.textContent).toBe(ratingMock.highest.toString());
        expect(minRating.textContent).toBe(ratingMock.lowest.toString());
    });

    it.each([
        [
            {
                ratings: [
                    { rating: 1000, achievedAt: new Date().toISOString() },
                ],
                current: 1100,
            },
            "+100",
            "text-green-400",
        ],
        [
            {
                ratings: [
                    { rating: 1000, achievedAt: new Date().toISOString() },
                ],
                current: 900,
            },
            "-100",
            "text-red-400",
        ],
        [
            {
                ratings: [
                    { rating: 1000, achievedAt: new Date().toISOString() },
                ],
                current: 1000,
            },
            "±0",
            "text-neutral-400",
        ],
    ])(
        "should display the correct rating change and color for %p",
        (partialOverview, expectedText, expectedClass) => {
            const ratingOverview = {
                ...ratingMock,
                ratings: partialOverview.ratings,
                current: partialOverview.current,
            };
            render(<RatingCard overview={ratingOverview} />);

            const change = screen.getByTestId("ratingChange");
            expect(change.textContent).toBe(expectedText);
            expect(change).toHaveClass(expectedClass);
        },
    );

    it("should fall back to current rating when there are no historical ratings", () => {
        ratingMock.ratings = [];

        render(<RatingCard overview={ratingMock} />);

        const change = screen.getByTestId("ratingChange");
        expect(change.textContent).toBe("±0");
    });
});
