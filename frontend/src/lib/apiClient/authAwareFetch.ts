import { logout, refresh } from "./definition";
import { navigate } from "@/actions/navigate";
import constants from "../constants";

type Pending = {
    resolve: (value: Response | PromiseLike<Response>) => void;
    reject: (reason?: unknown) => void;
    input: URL | RequestInfo;
    init?: RequestInit;
};

let isRefreshing = false;
let refreshQueue: Pending[] = [];
export default async function authAwareFetch(
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> {
    const response = await fetch(input, init);

    // if the server is making this request we don't want to auto refresh
    const isServerRequest = typeof window === "undefined";
    if (response.status !== 401 || isServerRequest) return response;

    if (response.status === 401 && isRefreshing)
        return addToRefreshQueue(input, init);

    const newResponse = addToRefreshQueue(input, init);
    isRefreshing = true;
    let isRefreshSuccessful: boolean = false;
    try {
        isRefreshSuccessful = await handleRefresh();
        refreshQueue.forEach(({ resolve, reject, input, init }) => {
            fetch(input, init).then(resolve).catch(reject);
        });
    } catch (err) {
        refreshQueue.forEach(({ reject }) => reject(err));
        throw err;
    } finally {
        refreshQueue = [];
        isRefreshing = false;

        if (!isRefreshSuccessful) {
            await logout();
            navigate(constants.PATHS.LOGIN);
        }
    }

    return newResponse;
}

async function handleRefresh(): Promise<boolean> {
    const response = await fetch(constants.PATHS.REFRESH);
    if (!response.ok) {
        console.error("Failed refreshing:", await response.text());
        return false;
    }

    return true;
}

const addToRefreshQueue = (
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> =>
    new Promise<Response>((resolve, reject) =>
        refreshQueue.push({ resolve, reject, input, init }),
    );
