import { PagedResultOfQuestPointsDto, UserQuestPoints } from "@/lib/apiClient";
import { createFakeMinimalProfile } from "./minimalProfileFaker";
import { faker } from "@faker-js/faker";
import { createFakePagedResult, FakePagedResultArgs } from "./pagedResultFaker";
import constants from "@/lib/constants";

export function createFakeUserQuestPoints(
    overrides?: Partial<UserQuestPoints>,
): UserQuestPoints {
    return {
        profile: createFakeMinimalProfile(),
        questPoints: faker.number.int({ min: 10, max: 100 }),
        ...overrides,
    };
}

export function createFakePagedUserQuestPoints({
    pagination,
    overrides,
}: {
    pagination?: Partial<FakePagedResultArgs>;
    overrides?: Partial<UserQuestPoints>;
} = {}): PagedResultOfQuestPointsDto {
    return createFakePagedResult({
        pageSize:
            pagination?.pageSize ??
            constants.PAGINATION_PAGE_SIZE.QUEST_LEADERBOARD,
        totalCount: pagination?.totalCount ?? 20,
        page: pagination?.page ?? 0,
        createFakeItem: () => createFakeUserQuestPoints(overrides),
    });
}
