import { Clocks } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeClock(overrides: Partial<Clocks> = {}): Clocks {
    return {
        whiteClock: faker.number.int({ min: 10000, max: 100000 }),
        blackClock: faker.number.int({ min: 10000, max: 100000 }),
        lastUpdated: Date.now().valueOf(),
        isFrozen: false,
        ...overrides,
    };
}
