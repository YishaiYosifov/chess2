import { ResponseError } from "../apiClient";
import { FormikErrors } from "formik";

interface ErrorMetadata {
    relatedField?: string;
}

interface ErrorDetail {
    code: string;
    detail: string;
    metadata?: ErrorMetadata;
}

export async function toFormikErrors<TValue>(
    error: ResponseError,
): Promise<FormikErrors<TValue>> {
    const json = await error.response.json();

    const formikErrors: FormikErrors<TValue> = {};
    for (const error of json.errors) {
        const field = error.metadata?.relatedField;
        if (field === undefined) continue;

        const formattedField = `${field[0].toLowerCase()}${field.slice(1)}`;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        formikErrors[formattedField as keyof TValue] = error.detail as any;
    }
    return formikErrors;
}
