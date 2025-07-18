interface PagedResult<TItem> {
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    items: TItem[];
}

export interface FakePagedResultArgs {
    pageSize: number;
    totalCount: number;
    page: number;
}

interface CreateFakePagedResultArgs<TItem> extends FakePagedResultArgs {
    createFakeItem: () => TItem;
}

export function createFakePagedResult<TItem>({
    pageSize,
    totalCount,
    page,
    createFakeItem,
}: CreateFakePagedResultArgs<TItem>): PagedResult<TItem> {
    const items: TItem[] = [];
    for (let i = 0; i < totalCount; i++) items.push(createFakeItem());
    const totalPages = Math.ceil(totalCount / pageSize);

    return {
        items,
        totalCount,
        page,
        pageSize,
        totalPages,
    };
}
