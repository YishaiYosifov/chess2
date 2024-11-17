import { FormikErrors } from "formik";

interface ErrorMetadata {
    relatedField?: string;
}

interface ErrorDetail {
    code: string;
    detail: string;
    metadata?: ErrorMetadata;
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

    toFormik<TValue>() {
        const formikErrors: FormikErrors<TValue> = {};
        for (const error of this.errors) {
            const field = error.metadata?.relatedField;
            if (field === undefined) continue;

            const formattedField = `${field[0].toLowerCase()}${field.slice(1)}`;
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
            formikErrors[formattedField as keyof TValue] = error.detail as any;
        }
        return formikErrors;
    }
}
