import { logout } from "./definition";
import { navigate } from "@/actions/navigate";
import constants from "../constants";
import handleRefresh from "./refresh";
import rawClient from "./rawClient";

export default async function authAwareFetch(
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> {
    const response = await fetch(input, init);

    // if the server is making this request we don't want to auto refresh
    const isServerRequest = typeof window === "undefined";
    if (response.status !== 401 || isServerRequest) return response;

    const isRefreshSuccessful = await handleRefresh();
    if (!isRefreshSuccessful) throw new Error();

    const newResponse = await fetch(input, init);
    if (newResponse.status === 401) {
        await logout({ client: rawClient });
        navigate(constants.PATHS.REGISTER);
    }

    return newResponse;
}
