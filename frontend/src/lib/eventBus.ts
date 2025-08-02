import { MaybePromise } from "@/types/types";

export type EventListener<TArgs extends unknown[], TResult = void> = (
    ...args: TArgs
) => MaybePromise<TResult>;

export class EventBus<TArgs extends unknown[], TResult = void> {
    public readonly listeners: Set<EventListener<TArgs, TResult>> = new Set();

    subscribe(fn: EventListener<TArgs, TResult>) {
        this.listeners.add(fn);
    }

    unsubscribe(fn: EventListener<TArgs, TResult>) {
        this.listeners.delete(fn);
    }

    async emit(...args: TArgs) {
        const results: TResult[] = [];
        for (const listener of this.listeners) {
            results.push(await listener(...args));
        }
        return results;
    }

    async emitUntilTruthy(...args: TArgs) {
        for (const listener of this.listeners) {
            const result = await listener(...args);
            if (result) return result;
        }
        return undefined;
    }
}
