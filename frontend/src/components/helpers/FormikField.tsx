import { useField } from "formik";
import React from "react";

type FormikFieldProps<TProps extends React.ComponentType> = {
    asInput: TProps;
    name: string;
} & React.ComponentProps<TProps>;

/**
 * Render a regular field as a formik field.
 * Adds error handling and formik field props.
 */
const FormikField = <TProps extends React.ComponentType>({
    asInput,
    name,
    ...props
}: FormikFieldProps<TProps>) => {
    const [field, meta] = useField(name);

    return (
        <>
            {/* eslint-disable-next-line @typescript-eslint/no-explicit-any */}
            {React.createElement(asInput as any, {
                ...field,
                ...props,
            })}

            {meta.error && <span className="text-error">{meta.error}</span>}
        </>
    );
};
export default FormikField;
