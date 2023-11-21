"use client";

import { Formik, FormikHelpers } from "formik";
import { Form } from "react-bootstrap";

import * as yup from "yup";

import styles from "./PasswordField.module.scss";

import withCollapsibleField from "./withCollapsibleField";
import { FormikField } from "@/components/FormField";
import SubmitField from "./helpers/SubmitField";

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
const PasswordField = withCollapsibleField<PasswordFieldProps>(
    ({
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
                initialValues={{
                    [field]: defaultValue,
                    passwordConfirm: "",
                }}
                onSubmit={onSubmit}
            >
                {({ handleSubmit, values, errors, isSubmitting, status }) => {
                    return (
                        <Form
                            noValidate
                            onSubmit={handleSubmit}
                            className={styles.form}
                        >
                            <FormikField
                                fieldName={field}
                                fieldLabel={fieldLabel}
                            />
                            <FormikField
                                fieldName="passwordConfirm"
                                fieldLabel="Password Confirm"
                                type="password"
                            />
                            <span className="text-invalid">{status}</span>

                            <SubmitField
                                disableSubmitOn={
                                    values[field] == defaultValue ||
                                    Object.keys(errors).length > 0 ||
                                    isSubmitting
                                }
                                onCancel={onCancel}
                            />
                        </Form>
                    );
                }}
            </Formik>
        );
    }
);
export default PasswordField;
