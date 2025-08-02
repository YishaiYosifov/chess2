export type MaybePromise<T> = Promise<T> | T;

export type Brand<T, B extends symbol> = T & {
    readonly [brand in B]: true;
};
