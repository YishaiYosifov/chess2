import { Quest, QuestDifficulty } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";

export function createFakeQuest(overrides?: Partial<Quest>): Quest {
    const target = overrides?.target ?? faker.number.int({ min: 5, max: 10 });
    return {
        difficulty: faker.helpers.enumValue(QuestDifficulty),
        description: faker.lorem.sentence(),
        progress: faker.number.int({ min: 0, max: target - 1 }),
        target,
        streak: faker.number.int({ min: 1, max: 10 }),
        canReplace: true,
        rewardPending: false,
        ...overrides,
    };
}
