import { DefaultConfig } from "./apiConfig";

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
        initOverrides?: RequestInit,
    ): Promise<Response> {
        const fetchContext = this.processRequestOptions(options, initOverrides);

        const response = await this.fetchApi(fetchContext);
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
        options: RequestOpts,
        initOverrides?: RequestInit,
    ): RequestContext {
        const headers = {
            ...this.config.headers,
            ...options.headers,
        };

        let processedBody: string;
        if (typeof options.body === "string") processedBody = options.body;
        else {
            // If the body is an object, it means we are sending json
            headers["Content-Type"] = "application/json";
            processedBody = JSON.stringify(options.body);
        }

        if (options.query)
            options.path += this.processQueryParams(options.query);

        const url = this.config.basePath + options.path;
        const init: RequestInit = {
            headers,
            credentials: this.config.credentials,
            method: options.method,
            body: processedBody,
            ...initOverrides,
        };

        return { url, init };
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
     * Sends the request to the api
     *
     * @param fetchContext - info about the request
     * @returns a promise that resolves to the api response
     */
    private async fetchApi(fetchContext: RequestContext) {
        if (this.config.preRequest)
            fetchContext =
                (await this.config.preRequest(fetchContext)) ?? fetchContext;

        let response = await fetch(fetchContext.url, fetchContext.init);

        if (this.config.postRequest) {
            response =
                (await this.config.postRequest(
                    fetchContext,
                    response.clone(),
                )) || response;
        }

        return response;
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

export type HTTPHeaders = Record<string, string>;

type QueryParams = Record<string, string | number | boolean | null>;

export enum HttpMethod {
    Get = "GET",
    Post = "POST",
    Patch = "PATCH",
    Put = "PUT",
    Delete = "DELETE",
}

interface ErrorDetail {
    code: string;
    detail: string;
}

export class ResponseError extends Error {
    constructor(
        public status: number,
        public title: string,
        public type: string,
        public errors: ErrorDetail[],
    ) {
        super(title);
    }

    static async fromResponse(response: Response): Promise<ResponseError> {
        const parsed = await response.json();

        return new ResponseError(
            parsed.status,
            parsed.title,
            parsed.type,
            parsed.errors,
        );
    }
}

export interface RequestOpts {
    path: string;
    method: HttpMethod;
    query?: QueryParams;
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
        return await this.response.json();
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
