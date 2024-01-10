import {
    Form,
    FormControlProps,
    FormSelectProps,
    InputGroup,
} from "react-bootstrap";

import { ComponentType, ReactNode } from "react";
import { FieldInputProps, FieldMetaProps, useField } from "formik";

interface HOCFormikElement {
    field: FieldInputProps<any>;
    meta: FieldMetaProps<any>;
}

interface HOCProps {
    fieldName: string;
    icon?: ReactNode;
}

/**
 * Higher order component that adds formik functionality to form components
 */
const withFormikElement = <T extends HOCFormikElement>(
    WrappedComponent: ComponentType<T>
) => {
    const FormikComponent = (
        props: Omit<T & HOCProps, keyof HOCFormikElement>
    ) => {
        const [field, meta] = useField(props.fieldName);

        return (
            <>
                <WrappedComponent {...(props as T)} meta={meta} field={field} />

                {props.icon && <InputGroup.Text>{props.icon}</InputGroup.Text>}
                <Form.Control.Feedback type="invalid">
                    {meta.error}
                </Form.Control.Feedback>
            </>
        );
    };
    return FormikComponent;
};

interface FormikElement extends HOCFormikElement {
    fieldName: string;
}

/**
 * Render input fields within a formik form
 */
export const FormikInput = withFormikElement(
    ({
        fieldName,
        children,
        field,
        meta,
        ...inputProps
    }: FormikElement & FormControlProps) => {
        return (
            <>
                <Form.Control
                    {...inputProps}
                    {...field}
                    aria-label={fieldName}
                    isInvalid={Boolean(meta.error)}
                />
                {children}
            </>
        );
    }
);

/**
 * Render select fields within a formik form
 */
export const FormikSelect = withFormikElement(
    ({
        fieldName,
        field,
        meta,
        children,
        ...inputProps
    }: FormikElement & FormSelectProps) => {
        return (
            <Form.Select
                {...inputProps}
                {...field}
                aria-label={fieldName}
                isInvalid={Boolean(meta.error)}
            >
                {children}
            </Form.Select>
        );
    }
);
