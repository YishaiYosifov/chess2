export default class AsyncLock {
    private queue: (() => void)[] = [];
    private locked = false;

    async acquire<T>(callback: () => Promise<T> | T): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            const run = async () => {
                try {
                    this.locked = true;
                    const result = await callback();
                    resolve(result);
                } catch (err) {
                    reject(err);
                } finally {
                    this.locked = false;
                    const next = this.queue.shift();
                    if (next) next();
                }
            };

            if (this.locked) this.queue.push(run);
            else run();
        });
    }
}
