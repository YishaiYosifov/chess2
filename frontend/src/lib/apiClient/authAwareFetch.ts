import { logout, refresh } from "./definition";
import { navigate } from "@/actions/navigate";
import constants from "../constants";

type Pending = {
    resolve: (value: Response | PromiseLike<Response>) => void;
    reject: (reason?: unknown) => void;
    input: URL | RequestInfo;
    init?: RequestInit;
};

const IGNORE_CONTROLLERS: string[] = ["/api/auth"];

let isRefreshing = false;
let refreshQueue: Pending[] = [];
export default async function authAwareFetch(
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> {
    const response = await fetch(input, init);

    // if the server is making this request we don't want to auto refresh
    const isServerRequest = typeof window === "undefined";

    const url = new URL(input.toString());
    const shouldIgnoreController = IGNORE_CONTROLLERS.some((path) =>
        url.pathname.toLocaleLowerCase().startsWith(path.toLowerCase()),
    );
    if (response.status !== 401 || isServerRequest || shouldIgnoreController)
        return response;

    if (isRefreshing) return addToRefreshQueue(input, init);

    const newResponse = addToRefreshQueue(input, init);
    isRefreshing = true;
    let isRefreshSuccessful: boolean = false;
    try {
        isRefreshSuccessful = await handleRefresh();
        refreshQueue.forEach(({ resolve, reject, input, init }) => {
            logoutWhenUnauthorizedFetch(input, init)
                .then(resolve)
                .catch(reject);
        });
    } catch (err) {
        refreshQueue.forEach(({ reject }) => reject(err));
        throw err;
    } finally {
        refreshQueue = [];
        isRefreshing = false;

        if (!isRefreshSuccessful) await handleLogout();
    }

    return newResponse;
}

async function logoutWhenUnauthorizedFetch(
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> {
    const response = await fetch(input, init);
    if (response.status === 401) await handleLogout();

    return response;
}

async function handleRefresh(): Promise<boolean> {
    const { error } = await refresh();
    if (error) {
        console.error("Failed refreshing:", error);
        return false;
    }

    return true;
}

async function handleLogout() {
    await logout();
    navigate(constants.PATHS.LOGIN);
}

const addToRefreshQueue = (
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> =>
    new Promise<Response>((resolve, reject) =>
        refreshQueue.push({ resolve, reject, input, init }),
    );
