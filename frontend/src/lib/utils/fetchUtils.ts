import * as fetchUtils from "./fetchUtils";

/**
 * Makes an API request to a specific route with JSON data.
 * If this is ran from a server component, the cookies will be added to the headers automatically
 *
 * @param route - the api route to send the request to
 * @param options - options object
 *
 * @param [options.method="POST"]
 * @param options.json - the JSON data to send to the API
 * @param options.csrfToken - explicitly define the csrf token
 * @param options.args - any other options to pass to the fetch request
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
        json?: any;
        csrfToken?: string;
        headers?: Headers;
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

    //if (method != "GET")
    //headers.set("X-CSRFToken", csrfToken || (await getCsrf()));

    const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}${route}`, {
        method,
        body,

        headers,
        credentials: "include",
        ...args,
    });
    return response;
}

/**
 * Fetch data from the api and convert it to camel case
 *
 * @param url - the url to fetch data from
 * @param options - an optional configuration object
 * @param options.dataProcessor - a function to run the response json through
 * @returns the json response as camel case
 */
export async function getResource<T>(
    url: string,
    {
        dataProcessor,
        ...options
    }: RequestInit & { dataProcessor?: (data: any) => any } = {
        dataProcessor: snakeToCamel,
        next: { revalidate: 60 },
    }
): Promise<T | null> {
    try {
        const request = await fetch(
            `${process.env.NEXT_PUBLIC_API_URL}${url}`,
            options
        );
        if (!request.ok) {
            console.error(`Could not fetch "${url}":`, await request.text());
            return null;
        }

        const json = await request.json();
        const processed = dataProcessor ? dataProcessor(json) : json;
        return processed;
    } catch (err) {
        console.error(`Could not fetch "${url}":`, err);
        return null;
    }
}

/**
 * Converts an object with snake_case keys to camelCase recursively.
 *
 * @param obj - the input object to be camelCased
 * @returns a new object with camelCase keys
 */
export function snakeToCamel(obj: any) {
    if (obj === null || typeof obj !== "object") return obj;
    if (Array.isArray(obj)) return obj.map((item): any => snakeToCamel(item));

    const camelCased: Record<any, any> = {};
    for (const [key, value] of Object.entries(obj)) {
        const camelKey = key.replace(/_([a-z])/g, (match: string, p1: string) =>
            p1.toUpperCase()
        );

        camelCased[camelKey] = snakeToCamel(value);
    }
    return camelCased;
}

/**
 * Process an array get requests
 *
 * @param name - the name of the parameter
 * @param array - the array to process
 * @returns the formatted object
 */
export function arrayToBody(name: string, array: Array<string>): string {
    const parts: Array<string> = [];
    array.forEach((listElement) => parts.push(`${name}=${listElement}`));

    return parts.join("&");
}
