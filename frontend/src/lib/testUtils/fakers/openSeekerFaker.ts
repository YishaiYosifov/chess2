import { OpenSeek } from "@/features/lobby/lib/types";
import { PoolType, TimeControl } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export default function createFakeOpenSeek(
    overrides: Partial<OpenSeek> = {},
): OpenSeek {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        pool: {
            poolType: PoolType.CASUAL,
            timeControl: {
                baseSeconds: faker.number.int({ min: 10, max: 1000 }),
                incrementSeconds: faker.number.int({ min: 1, max: 10 }),
            },
        },
        timeControl: faker.helpers.enumValue(TimeControl),
        ...overrides,
    };
}
