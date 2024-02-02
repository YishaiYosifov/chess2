import { render, screen } from "@testing-library/react";
import { Mock } from "vitest";

import { Chart } from "react-google-charts";

import { RatingOverview } from "@/client";
import { Variant } from "@/lib/constants";

import RatingCard from "../RatingsCard";

vi.mock("react-google-charts");

const ratingMock: RatingOverview = {
    history: [{ elo: 900, achievedAt: new Date("2023-01-01T12:00:00") }],
    current: 1000,
    min: 900,
    max: 1100,
};

describe("RatingsCard", () => {
    it("should render the card with variant, rating data, and chart", () => {
        render(
            <RatingCard variant={Variant.Anarchy} ratingData={ratingMock} />
        );

        expect(screen.queryByTestId("ratingChart")).toBeInTheDocument();
        expect(screen.queryByText(Variant.Anarchy)).toBeInTheDocument();
        expect(screen.queryByTestId("ratingInfoSection")).toBeInTheDocument();
    });

    it("should correctly render the rating history chart", () => {
        const currDate = new Date("2023-01-01T12:00:00");
        vi.setSystemTime(currDate);

        render(
            <RatingCard variant={Variant.Anarchy} ratingData={ratingMock} />
        );

        const chartMock = Chart as unknown as Mock;
        const data = chartMock.mock.calls[0][0].data;
        expect(data).toEqual([
            ["Date", "Elo"],
            ["1/1/2023, 12:00:00 PM", 900],
            ["1/1/2023, 12:00:00 PM", 1000],
        ]);
    });

    it("should display the provided rating information", () => {
        render(
            <RatingCard variant={Variant.Anarchy} ratingData={ratingMock} />
        );

        expect(screen.getByTestId("maxRating").textContent).toBe(
            ratingMock.max + ""
        );
        expect(screen.getByTestId("minRating").textContent).toBe(
            ratingMock.min + ""
        );
        expect(screen.getByTestId("currentRating").textContent).toBe(
            ratingMock.current + ""
        );
    });

    it.each([
        [1000, 1100, "text-danger", "-100"],
        [1000, 900, "text-success", "+100"],
        [1000, 1000, "", "±0"],
    ])(
        "should display the rating change",
        (current, previous, colorClass, expectedText) => {
            const newMockRating: RatingOverview = {
                ...ratingMock,
                current: current,
                history: [
                    {
                        elo: previous,
                        achievedAt: new Date("2023-01-01T12:00:00"),
                    },
                ],
            };
            render(
                <RatingCard
                    variant={Variant.Anarchy}
                    ratingData={newMockRating}
                />
            );

            const ratingChange = screen.getByTestId("ratingChange");
            expect(ratingChange.textContent).toBe(expectedText);
            expect(ratingChange.className).toBe(colorClass);
        }
    );
});
