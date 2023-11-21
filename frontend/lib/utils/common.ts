import * as yup from "yup";

import type { Game, PublicProfile, RatingsMap } from "@/lib/types";
import { getCsrf } from "./auth";

/**
 * Makes an API request to a specific route with JSON data.
 * If this is ran from a server component, the cookies will be added to the headers automatically
 *
 * @param route - the api route to send the request to
 * @param options - options object
 *
 * @param [options.method="POST"] - the HTTP method for the request
 * @param [options.headers={}] - the headers object. When json is provided, the content type will be added tot he headers automatically
 * @param [options.json] - the JSON data to send to the API
 *
 * @param [options.csrfToken] - explicitly define the csrf token
 *
 * @param [options.args] - any other options to pass to the fetch request
 * @returns a promise the resolves to the response from the api
 */
export async function apiRequest(
    route: string,
    {
        method = "POST",
        headers = new Headers(),
        json,

        csrfToken,
        ...args
    }: {
        method?: string;
        headers?: Headers;
        json?: any;

        csrfToken?: string;
    } & RequestInit = {}
): Promise<Response> {
    let body: string | null = null;
    if (json) {
        body = JSON.stringify(json);
        headers.set("Content-Type", "application/json");
        headers.set("Accept", "application/json");
    }

    // Check if this function is running inside a server component and add the cookies if so
    if (typeof window === "undefined") {
        const { cookies } = await import("next/headers");
        headers.set("Cookie", cookies().toString());
    }

    if (method != "GET")
        headers.set("X-CSRFToken", csrfToken || (await getCsrf()));

    const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api${route}`,
        {
            method,
            body,

            headers,
            credentials: "include",
            ...args,
        }
    );
    return response;
}

/**
 * Process an array for Flask WTF get form
 *
 * @param name - the name of the parameter
 * @param array - the array to process
 * @returns the formatted object
 */
export function arrayToBody(name: string, array: Array<string>): string {
    const parts: Array<string> = [];
    array.forEach((listElement, index) =>
        parts.push(`${name}-${index}=${listElement}`)
    );

    return parts.join("&");
}

export const titleString = (value: string): string =>
    value.charAt(0).toUpperCase() + value.replaceAll("_", " ").slice(1);

//#region Fetch user info

/**
 * Fetch a user's public profile by their username
 *
 * @param username - the username of the user to fetch
 * @returns a promise that resolves to the users profile or null if the user was not found
 */
export async function fetchProfile(
    username: string
): Promise<PublicProfile | null> {
    const profileRequest = await apiRequest(
        `/profile/${username}/info?include=${arrayToBody("include", [
            "username",
            "about",
            "pfpLastChanged",
        ])}`,
        {
            method: "GET",
            next: { revalidate: 60 },
        }
    );

    if (profileRequest.status == 404) return null;
    return ((await profileRequest.json())?.data || {}) as PublicProfile;
}

/**
 * Fetch a user's ratings since a specified date
 *
 * @param username - the username of the user whose ratings to fetch
 * @param since - the date since which to fetch ratings (in ISO date format)
 * @returns a promise that resolves to a mapping of game variants to rating data or null if the user was not found.
 */
export async function fetchRatings(
    username: string,
    since: string
): Promise<RatingsMap | null> {
    const ratingsRequest = await apiRequest(
        `/profile/${username}/ratings?since=${since}`,
        {
            method: "GET",
            next: { revalidate: 60 },
        }
    );

    if (ratingsRequest.status == 404) return null;
    return ((await ratingsRequest.json())?.data || {}) as RatingsMap;
}

/**
 * Fetch a user's games
 *
 * @param username - the username of the user whose games to fetch
 * @returns a promise that resolves to an array of game objects or null if the user was not found
 */
export async function fetchGames(username: string): Promise<Game[] | null> {
    const gamesRequest = await apiRequest(`/profile/${username}/games`, {
        method: "GET",
        next: { revalidate: 60 },
    });
    if (gamesRequest.status == 404) return null;

    return ((await gamesRequest.json())?.data || []) as Game[];
}

//#endregion

//#region Validators

yup.addMethod(yup.string, "username", function () {
    return this.required("Username must be between 1 and 30 characters")
        .test(
            "username-length",
            "Username must be between 1 and 30 characters",
            (value) => value.length <= 30
        )
        .test(
            "username-spaces",
            "Username can't include spaces",
            (value) => !value.includes(" ")
        )
        .test(
            "username-numbers",
            "Username can't be just numbers",
            (value) => !+value
        );
});

yup.addMethod(yup.string, "email", function () {
    return this.required("Email is required")
        .matches(/[\w\.-]+@[\w\.-]+(\.[\w]+)+/g, "Invalid Email")
        .max(256, "Email Too Long");
});

yup.addMethod(yup.string, "password", function () {
    return this.required("Password is required").matches(
        /^(?=.*[A-Z])(?=.*)(?=.*[0-9])(?=.*[a-z]).{8,}$/g,
        "Invalid Password"
    );
});

//#endregion
