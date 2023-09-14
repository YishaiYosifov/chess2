import { useStore } from "../store";
import { getCsrf } from "./auth";
import * as yup from "yup";

/* TODO: Error handling */
/**
 * Makes an API request to a specific route with JSON data
 *
 * @param {string} route - The api route to send the request to
 * @param {Object|null} options.json - The JSON data to send to the API
 * @param {string|null} option.authToken - An auth token to use instead of the one stored in the zustand store
 * @param {string} [options.method="POST"] - The HTTP method for the request
 * @param {string} [options.credentials="include"] - The credentials option
 * @param {Object|null} options.others - Any other options to pass to the fetch request
 * @returns {Promise<Response>} - A promise the resolves to the response from the api
 */
export async function apiRequest(
    route,
    {
        json,
        authToken,
        csrfToken,
        headers = {},
        method = "POST",
        credentials = "include",
        ...others
    } = {}
) {
    if (json) {
        json = JSON.stringify(json);
        headers["Content-Type"] = "application/json";
        headers.Accept = "application/json";
    }

    const { isAuthed, accessToken } = useStore.getState();
    if (isAuthed || authToken)
        headers.Authorization = "Bearer " + (authToken || accessToken);
    if (method != "GET")
        headers["X-CSRFToken"] = csrfToken || (await getCsrf());

    const response = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api${route}`,
        {
            method,
            body: json,

            headers,
            credentials,
            ...others,
        }
    );
    return response;
}

/**
 * Process an object for Flask WTF get form
 *
 * @param {Object} body - the body to process
 * @returns {string} - the formatted object
 */
export function processGetBody(body) {
    let parts = [];
    for (const [key, value] of Object.entries(body)) {
        if (!(value instanceof Array)) {
            parts.push(`${key}=${value}`);
            continue;
        }

        value.forEach((listElement, index) =>
            parts.push(`${key}-${index}=${listElement}`)
        );
    }

    return parts.join("&");
}

export const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

export async function fetchUser(selector, include) {
    const profileRequest = await apiRequest(
        `/profile/${selector}?${processGetBody({ include: include | [] })}`,
        { cache: "force-cache", next: { tags: [`user-${selector}`] } }
    );
    return await profileRequest.json();
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
            (value) => ![...value].every((char) => char >= 0 && char <= 9)
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
