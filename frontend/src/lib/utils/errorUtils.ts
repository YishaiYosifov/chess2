import type { FormikErrors } from "formik";

import type { ApiProblemDetails, ErrorCode } from "../client";

type ErrorMapping<TValue> = {
    [K in ErrorCode]?: keyof TValue;
};

export function mapErrorsToFormik<TValue>(
    problemDetails: ApiProblemDetails,
    errors: ErrorMapping<TValue>,
): FormikErrors<TValue> {
    const formikErrors: FormikErrors<TValue> = {};
    if (!problemDetails.errors) return formikErrors;

    for (const { errorCode, description } of problemDetails.errors) {
        const resolvedField = errors[errorCode];
        if (!resolvedField) continue;

        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        formikErrors[resolvedField] = description as any;
    }
    return formikErrors;
}
