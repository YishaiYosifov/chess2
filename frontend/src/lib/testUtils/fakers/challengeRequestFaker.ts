import { ChallengeRequest, TimeControl } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakeMinimalProfile } from "./minimalProfileFaker";
import { createFakePoolKey } from "./poolKeyFaker";

export function createFakeChallengeRequest(
    overrides?: Partial<ChallengeRequest>,
): ChallengeRequest {
    return {
        challengeToken: faker.string.alpha(16),
        requester: createFakeMinimalProfile(),
        recipient: createFakeMinimalProfile(),
        timeControl: faker.helpers.enumValue(TimeControl),
        pool: createFakePoolKey(),
        expiresAt: faker.date.future().toISOString(),
        ...overrides,
    };
}
