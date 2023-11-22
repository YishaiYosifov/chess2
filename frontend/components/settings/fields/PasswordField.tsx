"use client";

import { Formik, FormikHelpers } from "formik";
import { Form } from "react-bootstrap";

import * as yup from "yup";

import styles from "./PasswordField.module.scss";

import withCollapsibleField from "./withCollapsibleField";
import { FormikField } from "@/components/FormField";
import SubmitField from "./helpers/SubmitField";
import { apiRequest } from "@/lib/utils/common";

interface PasswordFieldProps {
    field: string;
    fieldLabel?: string | boolean;
    defaultValue?: any;
    schema: yup.ObjectSchema<any>;
    onSubmit: (values: Object, helpers: FormikHelpers<any>) => Promise<void>;
    onCancel: () => void;
}

interface PasswordFormValues {
    passwordConfirm: string;
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
    async function handlePasswordSubmit(
        values: PasswordFormValues,
        { setErrors, setStatus }: FormikHelpers<PasswordFormValues>
    ) {}

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
            onSubmit={handlePasswordSubmit}
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
                    <FormikField fieldName={field} fieldLabel={fieldLabel} />
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
            )}
        </Formik>
    );
};
export default withCollapsibleField<PasswordFieldProps>(PasswordField);
