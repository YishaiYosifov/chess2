import { PlayerSummary, PublicUser } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakePlayerSummary(
    overrides: Partial<PlayerSummary> = {},
): PlayerSummary {
    return {
        userId: faker.string.uuid(),
        isAuthenticated: true,
        userName: faker.internet.username(),
        rating: faker.number.int({ min: 1200, max: 2800 }),
        ...overrides,
    };
}

export const createFakePlayerSummaryFromUser = (user: PublicUser) =>
    createFakePlayerSummary({
        userId: user.userId,
        userName: user.userName ?? "",
    });
