import type { RequestContext, HTTPHeaders } from "./runtime";

export const BASE_PATH = "http://localhost";

export interface ConfigurationParameters {
    basePath?: string; // override base path

    preRequest?: PreRequest; // middleware to apply before/after fetch requests
    postRequest?: PostRequest;
    onError?: OnError;

    headers?: HTTPHeaders; //header params we want to use on every request
    credentials?: RequestCredentials; //value for the credentials param we want to use on each request
}

export class Configuration {
    constructor(private configuration: ConfigurationParameters = {}) {}

    set config(configuration: Configuration) {
        this.configuration = configuration;
    }

    get basePath(): string {
        return this.configuration.basePath ?? BASE_PATH;
    }

    get preRequest(): PreRequest | undefined {
        return this.configuration.preRequest;
    }

    get postRequest(): PostRequest | undefined {
        return this.configuration.postRequest;
    }

    get onError(): OnError | undefined {
        return this.configuration.onError;
    }

    get headers(): HTTPHeaders {
        return this.configuration.headers ?? {};
    }

    get credentials(): RequestCredentials {
        return this.configuration.credentials ?? "include";
    }
}

export const DefaultConfig = new Configuration();

type PreRequest = (context: RequestContext) => Promise<RequestContext | void>;
type PostRequest = (
    context: RequestContext,
    response: Response
) => Promise<Response | undefined>;
type OnError = (
    context: RequestContext,
    error: unknown
) => Promise<Response | undefined>;
