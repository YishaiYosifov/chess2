import { PoolKey, PoolType } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakePoolKey(overrides?: Partial<PoolKey>): PoolKey {
    return {
        poolType: faker.helpers.enumValue(PoolType),
        timeControl: {
            baseSeconds: faker.number.int({ min: 10, max: 1000 }),
            incrementSeconds: faker.number.int({ min: 1, max: 10 }),
        },
        ...overrides,
    };
}
