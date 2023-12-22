import { render, screen } from "@testing-library/react";

import { Chart } from "react-google-charts";

import { Variant } from "@/lib/constants";
import { RatingData } from "@/lib/types";

import RatingCard from "../RatingsCard";

jest.mock("react-google-charts", () => ({
    Chart: jest.fn((props) => <div data-testid="ratingChart" {...props} />),
}));

const ratingMock: RatingData = {
    history: [{ elo: 900, achievedAt: "2023-01-01T12:00:00" }],
    current: 1000,
    min: 900,
    max: 1100,
};

describe("RatingsCard", () => {
    it("should render the card with variant, rating data, and chart", () => {
        render(
            <RatingCard variant={Variant.Anarchy} ratingData={ratingMock} />
        );

        expect(screen.getByTestId("ratingChart")).toBeInTheDocument();
        expect(screen.getByText(Variant.Anarchy)).toBeInTheDocument();
        expect(screen.getByTestId("ratingInfoSection")).toBeInTheDocument();
    });

    it("should correctly render the rating history chart", () => {
        const currDate = new Date("2023-01-01T12:00:00");
        jest.useFakeTimers().setSystemTime(currDate);

        render(
            <RatingCard variant={Variant.Anarchy} ratingData={ratingMock} />
        );

        const chartMock = Chart as unknown as jest.Mock;
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
            const newMockRating = {
                ...ratingMock,
                current: current,
                history: [{ elo: previous, achievedAt: "2023-01-01T12:00:00" }],
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
