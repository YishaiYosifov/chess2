import * as yup from "yup";

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
        json?: any;
        headers?: Headers;

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

export const titleString = (value: string): string =>
    value.charAt(0).toUpperCase() + value.replaceAll("_", " ").slice(1);

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
