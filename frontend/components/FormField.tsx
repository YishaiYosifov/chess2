"use client";

import { Form, FormControlProps, InputGroup } from "react-bootstrap";

import { ReactNode } from "react";

import styles from "./FormField.module.scss";
import { titleString } from "@/lib/utils/common";
import { useField } from "formik";

export interface SettingsFieldProps extends FormControlProps {
    fieldName: string;
    fieldLabel?: string;
    hasValidation?: boolean;
    children?: ReactNode;
}

/**
 * Component for rendering form fields
 */
export const FormField = ({
    fieldName,
    children,
    fieldLabel = "",
    hasValidation = false,
    ...inputProps
}: SettingsFieldProps) => {
    fieldLabel ||= titleString(fieldName);

    return (
        <Form.Group className={styles["input-container"]}>
            <Form.Label>{fieldLabel}</Form.Label>

            <InputGroup hasValidation={hasValidation} className={styles.input}>
                <Form.Control aria-label={fieldName} {...inputProps} />
                {children}
            </InputGroup>
        </Form.Group>
    );
};

/**
 * Component for rendering form fields within a formik form
 */
export const FormikField = ({
    fieldName,
    children,
    ...inputProps
}: SettingsFieldProps) => {
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
