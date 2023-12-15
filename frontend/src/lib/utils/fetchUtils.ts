const API_URL = process.env.NEXT_PUBLIC_API_URL;

/**
 * Makes an API request to a specified route
 *
 * @param route - the api route to send the request to
 * @param options - options to pass to the fetch request
 * @returns a promise the resolves to the response from the api
 */
export async function apiRequest(
    route: string,
    { json, headers, ...options }: RequestInit & { json?: object } = {}
): Promise<Response | null> {
    try {
        headers = new Headers(headers);
        headers.set("Accept", "application/json");

        if (json) {
            headers.set("Content-Type", "application/json");
            options.body = JSON.stringify(json);
        }

        const response = await fetch(`${API_URL}${route}`, {
            credentials: "include",
            headers,
            ...options,
        });

        return response;
    } catch (err) {
        console.error(`Could not fetch ${route}:`, err);
        return null;
    }
}

/**
 * Fetch json data from the api and run it through the data processor
 *
 * @param route - the url to fetch data from
 * @param options - an optional configuration object
 * @param options.dataProcessor - a function to run the response json through
 * @returns the json response as camel case
 */
export async function getResource<T>(
    route: string,
    {
        dataProcessor,
        ...options
    }: RequestInit & { dataProcessor?: (data: any) => any } = {
        dataProcessor: snakeToCamel,
        next: { revalidate: 60 },
    }
): Promise<T | null> {
    const response = await apiRequest(route, options);
    if (!response) return null;
    else if (!response.ok) {
        console.error(
            `Bad response from ${route} (${response.status}):`,
            await response.text()
        );
        return null;
    }

    try {
        const json = await response.json();
        const processed = dataProcessor ? dataProcessor(json) : json;
        return processed;
    } catch (err) {
        console.log(`Could not decode response from ${route}:`, err);
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
