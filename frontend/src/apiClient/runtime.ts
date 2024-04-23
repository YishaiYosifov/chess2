import { DefaultConfig } from "./apiConfig";
import { UserIn } from "./models";

export class BaseAPI {
    constructor(protected config = DefaultConfig) {}

    /**
     * Make a request to the API
     *
     * @param options - basic info about the request
     * @param initOverrides - override the default fetch options
     * @returns a promise that resolves to the api response
     */
    async request(
        options: RequestOpts,
        initOverrides?: RequestInit
    ): Promise<Response> {
        const fetchContext = this.processRequestOptions(options, initOverrides);

        const response = await this.fetchApi(fetchContext);
        if (!response || response.status < 200 || response.status >= 300)
            throw new ResponseError(
                response,
                "Response returned an error code"
            );

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
        options: RequestOpts,
        initOverrides?: RequestInit
    ): RequestContext {
        const headers = {
            ...this.config.headers,
            ...options.headers,
        };

        // If the body is an object, it means we are sending json
        const bodyType = options.body?.constructor.name;
        if (bodyType === "Object") headers["Content-Type"] = "application/json";

        if (options.query)
            options.path += this.processQueryParams(options.query);

        const url = this.config.basePath + options.path;
        const init: RequestInit = {
            headers,
            credentials: this.config.credentials,
            method: options.method,
            body: options.body as any,
            ...initOverrides,
        };

        return { url, init };
    }

    /**
     * Parse a record into query params
     */
    private processQueryParams(queryParams: Record<string, any>): string {
        const queryString = Object.entries(queryParams).map(
            ([key, value]) => `${key}=${value}`
        );
        return "?" + queryString.join("&");
    }

    /**
     * Sends the request to the api
     *
     * @param fetchContext - info about the request
     * @returns a promise that resolves to the api response
     */
    private async fetchApi(fetchContext: RequestContext) {
        if (this.config.preRequest)
            fetchContext =
                (await this.config.preRequest(fetchContext)) ?? fetchContext;

        let response: Response;
        try {
            response = await fetch(fetchContext.url, fetchContext.init);
        } catch (e) {
            if (this.config.onError) {
                const newResponse = await this.config.onError(fetchContext, e);
                if (!newResponse) throw e;
                response = newResponse;
            } else throw e;
        }

        if (this.config.postRequest) {
            response =
                (await this.config.postRequest(
                    fetchContext,
                    response.clone()
                )) || response;
        }

        return response;
    }

    protected createFriendlyRoute<T extends any[], R>(
        rawFunc: (...args: T) => Promise<ApiResponse<R>>
    ): (...args: T) => Promise<R> {
        rawFunc = rawFunc.bind(this);

        return async (...args: T) => {
            const response = await rawFunc(...args);
            return await response.value();
        };
    }
}

export type HTTPHeaders = Record<string, string>;

export class ResponseError extends Error {
    override name: "ResponseError" = "ResponseError";
    constructor(public response: Response, msg?: string) {
        super(msg);
    }
}

export interface RequestOpts {
    path: string;
    method: string;
    query?: Record<string, any>;
    body?: string | object;
    headers?: HTTPHeaders;
}

export interface RequestContext {
    url: string;
    init: RequestInit;
}

export interface ApiResponse<T> {
    response: Response;
    value(): Promise<T>;
}

export class JSONApiResponse<T> {
    constructor(public response: Response) {}

    async value(): Promise<T> {
        return this.snakeToCamel(await this.response.json());
    }

    /**
     * Converts an object with snake_case keys to camelCase recursively.
     *
     * @param obj - the input object to be camelCased
     * @returns a new object with camelCase keys
     */
    snakeToCamel(obj: any): any {
        if (obj === null || typeof obj !== "object") return obj;
        if (Array.isArray(obj))
            return obj.map((item) => this.snakeToCamel(item));

        const camelCased: Record<any, any> = {};
        for (const [key, value] of Object.entries(obj)) {
            const camelKey = key.replace(
                /_([a-z])/g,
                (match: string, p1: string) => p1.toUpperCase()
            );

            camelCased[camelKey] = this.snakeToCamel(value);
        }
        return camelCased;
    }
}

export class VoidApiResponse {
    constructor(public response: Response) {}

    async value(): Promise<void> {
        return undefined;
    }
}

export class BlobApiResponse {
    constructor(public response: Response) {}

    async value(): Promise<Blob> {
        return await this.response.blob();
    }
}

export class TextApiResponse {
    constructor(public response: Response) {}

    async value(): Promise<string> {
        return await this.response.text();
    }
}
