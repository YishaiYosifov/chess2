"use client";

import { Form, FormControlProps, InputGroup } from "react-bootstrap";

import { ReactNode, forwardRef } from "react";

import styles from "./FormField.module.scss";

import { titleString } from "@/lib/utils/common";
import { useField } from "formik";

export interface SettingsFieldProps extends FormControlProps {
    fieldName?: string;
    fieldLabel?: string | boolean;
    hasValidation?: boolean;
    children?: ReactNode;
}

/**
 * Component for rendering form fields
 */
export const FormField = forwardRef<HTMLInputElement, SettingsFieldProps>(
    (
        {
            fieldName,
            children,
            fieldLabel,
            hasValidation = false,
            ...inputProps
        }: SettingsFieldProps,
        ref
    ) => {
        return (
            <Form.Group className={styles["input-container"]}>
                {fieldLabel && (
                    <Form.Label>
                        {typeof fieldLabel === "boolean"
                            ? titleString(fieldName ?? "")
                            : fieldLabel}
                    </Form.Label>
                )}

                <InputGroup
                    hasValidation={hasValidation}
                    ref={ref}
                    className={
                        (fieldLabel && styles["label-input"]) || undefined
                    }
                >
                    <Form.Control aria-label={fieldName} {...inputProps} />
                    {children}
                </InputGroup>
            </Form.Group>
        );
    }
);
FormField.displayName = "FormField";

/**
 * Component for rendering form fields within a formik form
 */
export const FormikField = ({
    fieldName,
    children,
    ...inputProps
}: SettingsFieldProps & { fieldName: string }) => {
    const [field, meta] = useField(fieldName);

    return (
        <FormField
            fieldName={fieldName}
            isInvalid={Boolean(meta.error)}
            hasValidation
            {...inputProps}
            {...field}
        >
            {children}

            <Form.Control.Feedback type="invalid">
                {meta.error}
            </Form.Control.Feedback>
        </FormField>
    );
};
