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

export class TextApiResponse {
    constructor(public response: Response) {}

    async value(): Promise<string> {
        return await this.response.text();
    }
}

export class BlobApiResponse {
    constructor(public response: Response) {}

    async value(): Promise<Blob> {
        return await this.response.blob();
    }
}
