import { GameSummary, PagedResultOfGameSummaryDto } from "@/lib/apiClient";
import { createFakePagedResult, FakePagedResultArgs } from "./pagedResultFaker";
import { createFakeGameSummary } from "./gameSummaryFaker";
import constants from "@/lib/constants";

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
