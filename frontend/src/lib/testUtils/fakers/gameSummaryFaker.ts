import { faker } from "@faker-js/faker";

import {
    GameResult,
    GameSummary,
    PagedResultOfGameSummaryDto,
} from "@/lib/apiClient";
import { createFakePagedResult, FakePagedResultArgs } from "./pagedResultFaker";
import { createFakePlayerSummary } from "./playerSummaryFaker";
import constants from "@/lib/constants";

export function createFakeGameSummary(
    override?: Partial<GameSummary>,
): GameSummary {
    return {
        gameToken: faker.string.uuid(),
        whitePlayer: createFakePlayerSummary(),
        blackPlayer: createFakePlayerSummary(),
        result: faker.helpers.enumValue(GameResult),
        createdAt: Date.now().toLocaleString(),
        ...override,
    };
}

export function createFakePagedGameSummary({
    pagination,
    overrides,
}: {
    pagination?: Partial<FakePagedResultArgs>;
    overrides?: Partial<GameSummary>;
} = {}): PagedResultOfGameSummaryDto {
    return createFakePagedResult({
        pageSize:
            pagination?.pageSize ?? constants.PAGINATION_PAGE_SIZE.GAME_SUMMARY,
        totalCount: pagination?.totalCount ?? 20,
        page: pagination?.page ?? 0,
        createFakeItem: () => createFakeGameSummary(overrides),
    });
}
