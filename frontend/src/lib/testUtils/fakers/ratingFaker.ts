import { Rating } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeRating(overrides?: Partial<Rating>): Rating {
    return {
        rating: faker.number.int({ min: 100, max: 3000 }),
        achievedAt: faker.date.anytime().toISOString(),
        ...overrides,
    };
}
