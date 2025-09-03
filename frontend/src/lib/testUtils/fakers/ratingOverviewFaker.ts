import { RatingOverview, TimeControl } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakeRating } from "./ratingFaker";

export function createFakeRatingOverview(
    overrides?: Partial<RatingOverview>,
): RatingOverview {
    const ratings =
        overrides?.ratings ??
        Array.from({
            length: 3,
        }).map(() => createFakeRating());

    return {
        timeControl: faker.helpers.enumValue(TimeControl),
        ratings: ratings,
        current: faker.number.int({ min: 100, max: 3000 }),
        highest: faker.number.int({ min: 100, max: 3000 }),
        lowest: faker.number.int({ min: 100, max: 3000 }),
        ...overrides,
    };
}
