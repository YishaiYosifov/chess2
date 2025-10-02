import { ChallengeRequest } from "@/lib/apiClient";
import { faker } from "@faker-js/faker";
import { createFakeMinimalProfile } from "./minimalProfileFaker";
import { createFakePoolKey } from "./poolKeyFaker";

export function createFakeChallengeRequets(
    overrides?: Partial<ChallengeRequest>,
): ChallengeRequest {
    return {
        challengeId: faker.string.alpha(16),
        requester: createFakeMinimalProfile(),
        recipient: createFakeMinimalProfile(),
        pool: createFakePoolKey(),
        expiresAt: faker.date.recent().toISOString(),
        ...overrides,
    };
}
