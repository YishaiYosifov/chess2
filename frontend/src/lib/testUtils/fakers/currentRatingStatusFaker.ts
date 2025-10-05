import { CurrentRatingStatus, TimeControl } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeCurrentRatingStatus(
    overrides?: Partial<CurrentRatingStatus>,
): CurrentRatingStatus {
    return {
        timeControl: faker.helpers.enumValue(TimeControl),
        rating: faker.number.int({ min: 100, max: 3000 }),
        ...overrides,
    };
}
