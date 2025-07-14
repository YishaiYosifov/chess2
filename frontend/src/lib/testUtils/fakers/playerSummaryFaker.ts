import { PlayerSummary, User } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakePlayerSummary(
    overrides: Partial<PlayerSummary> = {},
): PlayerSummary {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        rating: faker.number.int({ min: 1200, max: 2800 }),
        ...overrides,
    };
}

export const createFakePlayerSummaryFromUser = (user: User) =>
    createFakePlayerSummary({
        userId: user.userId,
        userName: user.userName ?? "",
    });
