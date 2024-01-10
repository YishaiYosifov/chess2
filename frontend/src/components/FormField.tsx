"use client";

import { Form, InputGroup } from "react-bootstrap";

import { ReactNode, forwardRef } from "react";

import styles from "./FormField.module.scss";

export interface SettingsFieldProps {
    fieldLabel?: string | boolean;
    hasValidation?: boolean;
    children?: ReactNode;
}

/**
 * Component for rendering form fields
 */
const FormField = forwardRef<HTMLInputElement, SettingsFieldProps>(
    (
        { children, fieldLabel, hasValidation = false }: SettingsFieldProps,
        ref
    ) => {
        return (
            <Form.Group className={styles["input-container"]}>
                {fieldLabel && (
                    <Form.Label data-testid="formFieldLabel">
                        {fieldLabel}
                    </Form.Label>
                )}

                <InputGroup
                    hasValidation={hasValidation}
                    ref={ref}
                    className={
                        (fieldLabel && styles["label-input"]) || undefined
                    }
                >
                    {children}
                </InputGroup>
            </Form.Group>
        );
    }
);
FormField.displayName = "FormField";

export default FormField;
