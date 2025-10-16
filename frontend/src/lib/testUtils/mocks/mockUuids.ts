export default function mockSequentialUUID(): void {
    let nextPieceId = 0;
    vi.stubGlobal("crypto", {
        randomUUID: () => (nextPieceId++).toString(),
    });
}
