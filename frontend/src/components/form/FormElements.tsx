import { useField, useFormik, useFormikContext } from "formik";

import Button from "../helpers/Button";
import React from "react";

export const SubmitButton = ({
    children,
    disabled,
    type,
    ...buttonProps
}: React.ButtonHTMLAttributes<HTMLButtonElement>) => {
    const { dirty, isValid, isSubmitting, status } = useFormikContext();

    return (
        <>
            <Button
                type={type ?? "submit"}
                disabled={disabled || isSubmitting || !dirty || !isValid}
                data-testid="submitFormButton"
                {...buttonProps}
            >
                {children}
            </Button>
            {status && <span className="text-error">{status}</span>}
        </>
    );
};

export const FormikErrorMessage = ({ name }: { name: string }) => {
    const { error } = useField(name)[1];
    return <>{error && <span className="text-error">{error}</span>}</>;
};

type FormikFieldProps<TProps extends React.ComponentType> = {
    asInput: TProps;
    name: string;
} & React.ComponentProps<TProps>;

/**
 * Render a regular field as a formik field.
 * Adds error handling and formik field props.
 */
export const FormikField = <TProps extends React.ComponentType>({
    asInput,
    name,
    ...props
}: FormikFieldProps<TProps>) => {
    const [field] = useField(name);

    return (
        <>
            {React.createElement(asInput as never, {
                ...field,
                ...props,
            })}

            <FormikErrorMessage name={name} />
        </>
    );
};
