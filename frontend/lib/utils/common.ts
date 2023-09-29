import type { Dictionary, PublicProfile } from "@/lib/types";
import { getCsrf } from "./auth";
import * as yup from "yup";

/**
 * Makes an API request to a specific route with JSON data.
 * If this is ran from a server component, the cookies will be added to the headers automatically
 *
 * @param route - The api route to send the request to
 * @param options - Options object
 *
 * @param [options.method="POST"] - The HTTP method for the request
 * @param [options.headers={}] - The headers object. When json is provided, the content type will be added tot he headers automatically
 * @param [options.json] - The JSON data to send to the API
 *
 * @param [options.csrfToken] - Explicitly define the csrf token
 *
 * @param [options.args] - Any other options to pass to the fetch request
 * @returns A promise the resolves to the response from the api
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
 * Process an object for Flask WTF get form
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

export const sleep = (ms: number) =>
    new Promise((resolve) => setTimeout(resolve, ms));

export async function fetchUser(username: string): Promise<PublicProfile> {
    const profileRequest = await apiRequest(`/profile/${username}`, {
        cache: "force-cache",
        next: { tags: [`user-${username}`] },
    });
    return await profileRequest.json();
}

export function uppercaseFirstLetter(value: string): string {
    return value.charAt(0).toUpperCase() + value.slice(1);
}

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
