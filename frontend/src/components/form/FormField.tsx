"use client";

import { Form, InputGroup } from "react-bootstrap";

import { ReactNode, forwardRef } from "react";

export interface SettingsFieldProps {
    label?: string | boolean;
    hasValidation?: boolean;
    children?: ReactNode;
}

/**
 * Component for rendering form fields
 */
const FormField = forwardRef<HTMLInputElement, SettingsFieldProps>(
    ({ children, label, hasValidation = false }, ref) => {
        return (
            <Form.Group>
                {label && (
                    <Form.Label data-testid="formFieldLabel">
                        {label}
                    </Form.Label>
                )}

                <InputGroup hasValidation={hasValidation} ref={ref}>
                    {children}
                </InputGroup>
            </Form.Group>
        );
    }
);
FormField.displayName = "FormField";

export default FormField;
