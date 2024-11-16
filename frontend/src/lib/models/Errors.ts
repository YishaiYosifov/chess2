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
