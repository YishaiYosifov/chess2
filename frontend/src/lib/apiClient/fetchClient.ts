import { refresh } from "./definition";

type Pending = {
    resolve: (value: Response | PromiseLike<Response>) => void;
    reject: (reason?: unknown) => void;
    input: URL | RequestInfo;
    init?: RequestInit;
};

let isRefreshing = false;
let refreshQueue: Pending[] = [];
export default async function customFetch(
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> {
    const response = await fetch(input, init);
    if (response.status !== 401 || input.toString().includes("refresh"))
        return response;

    if (response.status === 401 && isRefreshing)
        return addToRefreshQueue(input, init);

    const newResponse = addToRefreshQueue(input, init);
    isRefreshing = true;
    try {
        await refresh();
        refreshQueue.forEach(({ resolve, reject, input, init }) => {
            fetch(input, init).then(resolve).catch(reject);
        });
    } catch (err) {
        refreshQueue.forEach(({ reject }) => reject(err));
        throw err;
    } finally {
        refreshQueue = [];
        isRefreshing = false;
    }

    return newResponse;
}

const addToRefreshQueue = (
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> =>
    new Promise<Response>((resolve, reject) =>
        refreshQueue.push({ resolve, reject, input, init }),
    );
