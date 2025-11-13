import { PagedResultOfWinStreakDto, WinStreak } from "@/lib/apiClient";
import { createFakeMinimalProfile } from "./minimalProfileFaker";
import { faker } from "@faker-js/faker";
import { createFakePagedResult, FakePagedResultArgs } from "./pagedResultFaker";
import constants from "@/lib/constants";

export function createFakeWinStreak(overrides?: Partial<WinStreak>): WinStreak {
    return {
        profile: createFakeMinimalProfile(),
        highestStreakGameTokens: Array.from<string>({
            length: faker.number.int({ min: 1, max: 10 }),
        }).map(() => faker.string.alphanumeric(16)),
        ...overrides,
    };
}

export function createFakePagedWinStreak({
    pagination,
    overrides,
}: {
    pagination?: Partial<FakePagedResultArgs>;
    overrides?: Partial<WinStreak>;
} = {}): PagedResultOfWinStreakDto {
    return createFakePagedResult({
        pageSize:
            pagination?.pageSize ??
            constants.PAGINATION_PAGE_SIZE.WIN_STREAK_LEADERBOARD,
        totalCount: pagination?.totalCount ?? 20,
        page: pagination?.page ?? 0,
        createFakeItem: () => createFakeWinStreak(overrides),
    });
}
