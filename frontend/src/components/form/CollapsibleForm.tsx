"use client";

import { ForwardRefRenderFunction, forwardRef, useState } from "react";
import { Button, Form, FormProps, InputGroup } from "react-bootstrap";
import { BsPencilFill } from "react-icons/bs";

import styles from "./CollapsibleForm.module.scss";

import FormField from "@/components/form/FormField";
import { useFormikContext } from "formik";

export interface CollapsibleFormProps extends FormProps {
    label?: string;
    defaultValue?: string;
    disabled?: boolean;
}

/**
 * A form component that is able to be opened and collapsed.
 * Requires formik context.
 */
const CollapsibleForm: ForwardRefRenderFunction<
    HTMLFormElement,
    CollapsibleFormProps
> = ({ label, defaultValue, disabled, children, ...formProps }, ref) => {
    const [isEditing, setIsEditing] = useState(false);
    const { dirty, isValid, isSubmitting } = useFormikContext();

    const cancel = () => setIsEditing(false);
    const open = () => setIsEditing(true);

    if (isEditing)
        return (
            <Form {...formProps} className={styles["form"]} ref={ref}>
                {children}

                <div className={styles["submit-container"]}>
                    <Button
                        type="submit"
                        variant="dark"
                        disabled={!dirty || !isValid || isSubmitting}
                    >
                        Save
                    </Button>

                    <Button variant="dark" onClick={cancel}>
                        Cancel
                    </Button>
                </div>
            </Form>
        );

    return (
        <FormField label={label}>
            <Form.Control aria-label={label} value={defaultValue} disabled />

            {/* the button that enables and disables the form */}
            <InputGroup.Text>
                <button className="reset" onClick={open} disabled={disabled}>
                    <BsPencilFill />
                </button>
            </InputGroup.Text>
        </FormField>
    );
};

export default forwardRef(CollapsibleForm);
