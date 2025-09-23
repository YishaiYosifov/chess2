import LastOneWinsAsyncLock from "../lastOneWinsAsyncLock";

describe("LastOneWinsAsyncLock", () => {
    let lock: LastOneWinsAsyncLock;

    beforeEach(() => {
        lock = new LastOneWinsAsyncLock();
    });

    it("should acquire the lock and run a single async function", async () => {
        const result = await lock.acquire(async () => 123);
        expect(result).toBe(123);
    });

    it("should run the first task immediately and only the last queued while locked", async () => {
        const results: number[] = [];

        const asyncFunc = (value: number) =>
            lock.acquire(async () => {
                await new Promise((res) => setTimeout(res, 10));
                results.push(value);
            });

        await Promise.all([asyncFunc(1), asyncFunc(2), asyncFunc(3)]);

        // first task runs immediately, last queued while locked runs after
        expect(results).toEqual([1, 3]);
    });

    it("should propagate errors thrown inside the callback", async () => {
        await expect(
            lock.acquire(() => {
                throw new Error("fail");
            }),
        ).rejects.toThrow("fail");
    });

    it("should not block subsequent tasks after an error", async () => {
        const results: string[] = [];

        const failingFunc = () =>
            lock.acquire(() => {
                throw new Error("fail");
            });
        const succeedingFunc = () =>
            lock.acquire(async () => {
                results.push("success");
            });

        await expect(failingFunc()).rejects.toThrow("fail");
        await succeedingFunc();

        expect(results).toEqual(["success"]);
    });

    it("should skip intermediate queued functions and resolve their promises immediately", async () => {
        const results: number[] = [];

        const task1 = lock.acquire(async () => {
            results.push(1);
            await new Promise((res) => setTimeout(res, 10));
        });
        const task2 = lock.acquire(async () => {
            results.push(2);
        });
        const task3 = lock.acquire(async () => {
            results.push(3);
        });

        await Promise.all([task1, task2, task3]);

        // only task1 and task3 actually run
        expect(results).toEqual([1, 3]);
    });
});
