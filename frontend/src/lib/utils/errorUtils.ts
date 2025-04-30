import type { FormikErrors } from "formik";

import type { ValidationProblemDetails } from "../client";

export async function toFormikErrors<TValue>(
    problemDetails: ValidationProblemDetails,
): Promise<FormikErrors<TValue>> {
    const formikErrors: FormikErrors<TValue> = {};
    if (!problemDetails.errors) return formikErrors;

    for (const [field, errors] of Object.entries(problemDetails.errors)) {
        const formattedField = `${field[0].toLowerCase()}${field.slice(1)}`;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        formikErrors[formattedField as keyof TValue] = errors[0] as any;
    }
    return formikErrors;
}
