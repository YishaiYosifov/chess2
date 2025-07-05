import { render, screen } from "@testing-library/react";
import Chart from "react-apexcharts";
import { Mock } from "vitest";

import preloadAll from "@/lib/testUtils/dynamicImportMock";

import RatingCard from "../RatingsCard";
import { RatingOverview } from "@/types/tempModels";

vi.mock("react-apexcharts");

const ratingMock: RatingOverview = {
    history: [
        { elo: 800, achievedAt: new Date("2024-11-24T11:00:00").getTime() },
        { elo: 900, achievedAt: new Date("2024-11-24T12:00:00").getTime() },
    ],
    current: 1000,
    max: 1100,
};

describe("RatingsCard", () => {
    beforeAll(() => preloadAll());

    it("should render the card with variant, rating data, and chart", () => {
        render(<RatingCard ratingData={ratingMock} />);

        expect(screen.getByTestId("chart")).toBeInTheDocument();
        expect(screen.queryByTestId("ratingInfoSection")).toBeInTheDocument();
    });

    it("should correctly render the rating history chart", () => {
        const currDate = new Date("2024-11-24T12:00:00");
        vi.setSystemTime(currDate);

        render(<RatingCard ratingData={ratingMock} />);

        const chartMock = Chart as unknown as Mock;
        const data = chartMock.mock.calls[0][0].series;
        expect(data).toEqual([
            {
                name: "Elo",
                data: [
                    ...ratingMock.history.map((rating) => ({
                        x: rating.achievedAt,
                        y: rating.elo,
                    })),
                    {
                        x: currDate.getTime(),
                        y: ratingMock.current,
                    },
                ],
            },
        ]);
    });

    it("should display the provided rating information", () => {
        render(<RatingCard ratingData={ratingMock} />);

        expect(screen.getByTestId("maxRating").textContent).toBe(
            ratingMock.max.toString(),
        );
        expect(screen.getByTestId("currentRating").textContent).toBe(
            ratingMock.current.toString(),
        );
    });

    it.each([
        [1000, 1100, "-100"],
        [1000, 900, "+100"],
        [1000, 1000, "±0"],
    ])(
        "should display the rating change",
        (current, previous, expectedText) => {
            const newMockRating: RatingOverview = {
                ...ratingMock,
                current: current,
                history: [
                    {
                        elo: previous,
                        achievedAt: new Date("2023-01-01T12:00:00").getTime(),
                    },
                ],
            };
            render(<RatingCard ratingData={newMockRating} />);

            const ratingChange = screen.getByTestId("ratingChange");
            expect(ratingChange.textContent).toBe(expectedText);
        },
    );
});
