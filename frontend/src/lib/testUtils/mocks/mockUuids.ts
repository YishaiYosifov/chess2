export default function mockSequentialUUID({
    startAt,
}: { startAt?: number } = {}): void {
    let nextPieceId = startAt ?? 0;
    vi.stubGlobal("crypto", {
        randomUUID: () => (nextPieceId++).toString(),
    });
}
