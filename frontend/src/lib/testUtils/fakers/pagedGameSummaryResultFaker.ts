import { GameSummary, PagedResultOfGameSummaryDto } from "@/lib/apiClient";
import { createFakeGameSummary } from "./gameSummaryFaker";
import { faker } from "@faker-js/faker";

export function createFakePagedGameSummaryResult({
    count,
    overrides,
}: {
    count: number;
    overrides?: Partial<PagedResultOfGameSummaryDto>;
}): PagedResultOfGameSummaryDto {
    const pageSize =
        overrides?.pageSize ?? faker.number.int({ min: 1, max: 3 });
    const games: GameSummary[] = [];
    for (let i = 0; i < count; i++) {
        games.push(createFakeGameSummary());
    }

    const totalPages = Math.ceil(count / pageSize);
    const page =
        overrides?.page ?? faker.number.int({ min: 0, max: totalPages - 1 });

    return {
        items: games,
        totalCount: count,
        pageSize,
        totalPages,
        page,
        ...overrides,
    };
}
