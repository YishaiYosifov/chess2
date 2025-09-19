import AsyncLock from "../asyncLock";

describe("AsyncLock", () => {
    let lock: AsyncLock;

    beforeEach(() => {
        lock = new AsyncLock();
    });

    it("should acquire the lock and run a single async function", async () => {
        const result = await lock.acquire(async () => 123);
        expect(result).toBe(123);
    });

    it("should run multiple async functions in sequence", async () => {
        const results: number[] = [];

        const asyncFunc = (value: number) =>
            lock.acquire(async () => {
                await new Promise((res) => setTimeout(res, 10));
                results.push(value);
            });

        await Promise.all([asyncFunc(1), asyncFunc(2), asyncFunc(3)]);

        expect(results).toEqual([1, 2, 3]);
    });

    it("should propagate errors thrown inside the callback", async () => {
        await expect(
            lock.acquire(() => {
                throw new Error("fail");
            }),
        ).rejects.toThrow("fail");
    });

    it("should not block other functions after an error", async () => {
        const results: string[] = [];

        const failingFunc = () =>
            lock.acquire(() => {
                throw new Error("fail");
            });
        const succeedingFunc = () =>
            lock.acquire(() => results.push("success"));

        await expect(failingFunc()).rejects.toThrow("fail");
        await succeedingFunc();

        expect(results).toEqual(["success"]);
    });

    it("should process queued functions in order", async () => {
        const order: number[] = [];

        await Promise.all([
            lock.acquire(async () => {
                order.push(1);
            }),
            lock.acquire(async () => {
                order.push(2);
            }),
            lock.acquire(async () => {
                order.push(3);
            }),
        ]);

        expect(order).toEqual([1, 2, 3]);
    });
});
