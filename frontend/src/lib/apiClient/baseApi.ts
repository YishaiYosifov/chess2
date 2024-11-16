import { ResponseError } from "../models/Errors";
import { ApiResponse } from "./apiResponse";

type QueryParams = Record<string, string | number | boolean | null>;

export interface RequestOptions extends Omit<RequestInit, "body"> {
    query?: QueryParams;
    body?: string | object;
}

export interface RequestContext {
    url: string;
    init: RequestInit;
}

export class BaseAPI {
    constructor(protected readonly basePath: string) {}

    /**
     * Make a request to the API
     *
     * @param options - basic info about the request
     * @param initOverrides - override the default fetch options
     * @returns a promise that resolves to the api response
     */
    async request(
        path: string,
        options: RequestOptions = {},
    ): Promise<Response> {
        const fetchContext = this.processRequestOptions(path, options);

        const response = await fetch(fetchContext.url, fetchContext.init);
        if (!response || response.status < 200 || response.status >= 300)
            throw await ResponseError.fromResponse(response);

        return response;
    }

    /**
     * Combines the options and init overrides into a request context
     *
     * @param options - basic info about the request
     * @param initOverrides - override the default fetch options
     * @returns the created context of the request
     */
    private processRequestOptions(
        path: string,
        options: RequestOptions,
    ): RequestContext {
        const body = this.processBody(options);
        const init: RequestInit = {
            ...options,
            body,
            credentials: "include",
        };

        return { url: this.processPath(path, options), init };
    }

    /**
     * Process the request options body into a string.
     * Adds content type if necessary
     */
    private processBody(options: RequestOptions): string | undefined {
        if (!options.body) return;
        else if (typeof options.body === "string") return options.body;

        options.headers = new Headers(options.headers);
        options.headers.set("Content-Type", "application/json");
        return JSON.stringify(options.body);
    }

    /**
     * Converts a relative path into a full url with request params
     */
    private processPath(path: string, options: RequestOptions): string {
        if (options.query) path += this.processQueryParams(options.query);
        return this.basePath + path;
    }

    /**
     * Parse a record into query params
     */
    private processQueryParams(queryParams: QueryParams): string {
        const queryString = Object.entries(queryParams).map(
            ([key, value]) => `${key}=${value}`,
        );
        return "?" + queryString.join("&");
    }

    /**
     * Creates a method that returns the value of the api response
     * instead of the response object
     *
     * @param rawFunc - the method that sends the api request
     */
    protected createFriendlyRoute<TArgs extends unknown[], TReturn>(
        rawFunc: (...args: TArgs) => Promise<ApiResponse<TReturn>>,
    ): (...args: TArgs) => Promise<TReturn> {
        rawFunc = rawFunc.bind(this);

        return async (...args: TArgs) => {
            const response = await rawFunc(...args);
            return await response.value();
        };
    }
}
