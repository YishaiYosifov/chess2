export interface PagedResult<TItem> {
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages: number;
    items: TItem[];
}
