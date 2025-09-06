import { faker } from "@faker-js/faker";

import { createFakePagedResult, FakePagedResultArgs } from "./pagedResultFaker";
import { MinimalProfile, PagedResultOfMinimalProfile } from "@/lib/apiClient";
import constants from "@/lib/constants";

export function createFakeMinimalProfile(
    overrides?: Partial<MinimalProfile>,
): MinimalProfile {
    return {
        userId: faker.string.uuid(),
        userName: faker.internet.username(),
        ...overrides,
    };
}

export function createFakePagedStars({
    pagination,
    overrides,
}: {
    pagination?: Partial<FakePagedResultArgs>;
    overrides?: Partial<MinimalProfile>;
} = {}): PagedResultOfMinimalProfile {
    return createFakePagedResult({
        pageSize: pagination?.pageSize ?? constants.PAGINATION_PAGE_SIZE.STARS,
        totalCount: pagination?.totalCount ?? 10,
        page: pagination?.page ?? 0,
        createFakeItem: () => createFakeMinimalProfile(overrides),
    });
}

export function createFakePagedBlocked({
    pagination,
    overrides,
}: {
    pagination?: Partial<FakePagedResultArgs>;
    overrides?: Partial<MinimalProfile>;
} = {}): PagedResultOfMinimalProfile {
    return createFakePagedResult({
        pageSize:
            pagination?.pageSize ?? constants.PAGINATION_PAGE_SIZE.BLOCKED,
        totalCount: pagination?.totalCount ?? 10,
        page: pagination?.page ?? 0,
        createFakeItem: () => createFakeMinimalProfile(overrides),
    });
}
