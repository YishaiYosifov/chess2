import { faker } from "@faker-js/faker";

import { ChallengeRequest } from "@/lib/apiClient";
import { createFakeMinimalProfile } from "./minimalProfileFaker";
import { createFakePoolKey } from "./poolKeyFaker";

export function createFakeChallengeRequest(
    overrides?: Partial<ChallengeRequest>,
): ChallengeRequest {
    return {
        challengeToken: faker.string.alpha(16),
        requester: createFakeMinimalProfile(),
        recipient: createFakeMinimalProfile(),
        pool: createFakePoolKey(),
        expiresAt: faker.date.future().toISOString(),
        ...overrides,
    };
}
