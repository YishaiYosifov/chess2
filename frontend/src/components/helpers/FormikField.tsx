import { useField } from "formik";
import React from "react";

import FormikErrorMessage from "./FormikErrorMessage";

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
export default FormikField;
