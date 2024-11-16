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
            {React.createElement(asInput as never, {
                ...field,
                ...props,
            })}

            {meta.error && <span className="text-error">{meta.error}</span>}
        </>
    );
};
export default FormikField;
