import type { FormikErrors } from "formik";

import { ApiProblemDetails, ErrorCode } from "../apiClient";

type ErrorMapping<TValue> = {
    [K in keyof TValue]?: {
        mapping: ErrorCode[];
        default?: string;
    };
};

export function mapErrorsToFormik<T>(
    problemDetails: ApiProblemDetails,
    mapping: ErrorMapping<T>,
): FormikErrors<T> {
    const formikErrors: FormikErrors<T> = {};
    if (!problemDetails.errors) return formikErrors;

    const errorCodes = new Map(
        problemDetails.errors.map((e) => [e.errorCode, e.description]),
    );

    for (const field in mapping) {
        const fieldConfig = mapping[field];
        if (!fieldConfig) continue;

        const matched = fieldConfig.mapping.find((code) =>
            errorCodes.has(code),
        );

        const description = matched
            ? errorCodes.get(matched)
            : fieldConfig.default;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        formikErrors[field] = description as any;
    }

    return formikErrors;
}

export function findMatchingError(
    problemDetails: ApiProblemDetails,
    errorCodes: Set<ErrorCode>,
    defaultError: string,
) {
    const matched = problemDetails.errors.find((x) =>
        errorCodes.has(x.errorCode),
    );
    return matched ? matched.description : defaultError;
}
