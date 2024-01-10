"use client";

import { BsPencilFill } from "react-icons/bs";
import { Form, InputGroup } from "react-bootstrap";

import { ComponentType, useState } from "react";

import FormField from "@/components/FormField";

// Props for the HOC itself. They will not be passed down to the component
interface HOCProps {
    field: string;
    defaultValue?: string;
    disabled?: boolean;
}

// Props for the component that are provided by the HOC
interface HOCComponentProps {
    onCancel: () => void;
}

/**
 * A higher order component that adds collapsibility functionality to forms
 */
const withCollapsibleField = <T extends HOCComponentProps>(
    WrappedComponent: ComponentType<T>
) => {
    const InputComponent = (
        props: Omit<T & HOCProps, keyof HOCComponentProps>
    ) => {
        const [isEditing, setIsEditing] = useState(false);

        if (isEditing)
            return (
                <WrappedComponent
                    {...(props as T)}
                    onCancel={() => setIsEditing(false)}
                />
            );

        return (
            <FormField fieldLabel={props.field}>
                <Form.Control
                    aria-label={props.field}
                    disabled
                    value={props.defaultValue}
                />

                {/* the button that enables and disables the form */}
                <InputGroup.Text>
                    <button
                        className="reset"
                        onClick={() => setIsEditing(true)}
                        disabled={props.disabled}
                    >
                        <BsPencilFill />
                    </button>
                </InputGroup.Text>
            </FormField>
        );
    };
    return InputComponent;
};
export default withCollapsibleField;
