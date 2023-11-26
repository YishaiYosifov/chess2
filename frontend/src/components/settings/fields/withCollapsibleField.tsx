"use client";

import { BsPencilFill } from "react-icons/bs";
import { InputGroup } from "react-bootstrap";

import { ComponentType, useState } from "react";

import { FormField } from "@/components/FormField";

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
const withCollapsibleField = <P extends HOCComponentProps>(
    WrappedComponent: ComponentType<P>
) => {
    type HOCWithComponentProps = P & HOCProps;

    /**
     * The enchanced component with collapsible functionality
     */
    return (props: Omit<HOCWithComponentProps, keyof HOCComponentProps>) => {
        const [isEditing, setIsEditing] = useState(false);

        if (isEditing)
            return (
                <WrappedComponent
                    {...(props as HOCWithComponentProps)}
                    onCancel={() => setIsEditing(false)}
                />
            );

        return (
            <FormField
                fieldName={props.field}
                value={props.defaultValue}
                fieldLabel
                disabled
            >
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
};
export default withCollapsibleField;
