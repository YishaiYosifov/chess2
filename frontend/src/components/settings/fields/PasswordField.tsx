"use client";

import { Formik, FormikHelpers } from "formik";
import { Button, Form } from "react-bootstrap";

import * as yup from "yup";

import styles from "./PasswordField.module.scss";

import withCollapsibleField from "./withCollapsibleField";
import { FormikInput } from "@/components/FormikElements";
import FormField from "@/components/FormField";

interface PasswordFieldProps {
    field: string;
    fieldLabel?: string;
    defaultValue?: any;
    schema: yup.ObjectSchema<any>;
    onSubmit: (values: Object, helpers: FormikHelpers<any>) => Promise<void>;
    onCancel: () => void;
}

/**
 * Helper component to create a field that requires a password
 */
const PasswordField = ({
    field,
    fieldLabel,
    defaultValue,
    schema,
    onSubmit,
    onCancel,
}: PasswordFieldProps) => {
    const passwordSchema = yup
        .object()
        .shape({
            passwordConfirm: yup
                .string()
                .required("Password confirm is required"),
        })
        .concat(schema);

    return (
        <Formik
            validationSchema={passwordSchema}
            onSubmit={onSubmit}
            initialValues={{
                [field]: defaultValue,
                passwordConfirm: "",
            }}
        >
            {({ handleSubmit, values, errors, isSubmitting, status }) => (
                <Form
                    noValidate
                    onSubmit={handleSubmit}
                    className={styles.form}
                >
                    <FormField fieldLabel={fieldLabel} hasValidation>
                        <FormikInput fieldName={field} />
                    </FormField>

                    <FormField fieldLabel="Password Confirm" hasValidation>
                        <FormikInput fieldName="password" type="password" />
                    </FormField>
                    <span className="text-invalid">{status}</span>

                    <div className={styles["submit-container"]}>
                        <Button
                            type="submit"
                            variant="dark"
                            disabled={
                                values[field] == defaultValue ||
                                Object.keys(errors).length > 0 ||
                                isSubmitting
                            }
                        >
                            Save
                        </Button>

                        <Button variant="dark" onClick={onCancel}>
                            Cancel
                        </Button>
                    </div>
                </Form>
            )}
        </Formik>
    );
};
export default withCollapsibleField<PasswordFieldProps>(PasswordField);
