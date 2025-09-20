export default class LastOneWinsAsyncLock {
    private next: (() => void) | null = null;
    private nextResolve: ((value?: undefined) => void) | null = null;
    private locked = false;

    async acquire<T>(callback: () => Promise<T>): Promise<T | undefined> {
        return new Promise((resolve, reject) => {
            const run = async () => {
                try {
                    this.locked = true;
                    const result = await callback();
                    resolve(result);
                } catch (err) {
                    reject(err);
                } finally {
                    this.locked = false;
                    const nextFn = this.next;
                    this.next = null;
                    nextFn?.();
                }
            };

            if (this.locked) {
                this.nextResolve?.();
                this.next = run;
                this.nextResolve = resolve;
            } else {
                run();
            }
        });
    }
}
