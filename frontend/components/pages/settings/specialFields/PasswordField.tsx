import { Card, Form, InputGroup } from "react-bootstrap";
import { Formik, FormikHelpers } from "formik";

import { useState } from "react";
import * as yup from "yup";

import { uppercaseFirstLetter } from "@/lib/utils/common";

import PasswordConfirmField from "./helpers/PasswordConfirmField";
import FieldToggler from "./helpers/FieldToggler";
import SubmitRow from "./helpers/SubmitRow";

// Helper component to create a field that requires a password
const PasswordField = ({
    field,
    defaultValue = "",
    editingError = "",
    schema,
    onSubmit,
}: {
    field: string;
    defaultValue?: string;
    editingError?: string | boolean;
    schema: yup.ObjectSchema<any>;
    onSubmit: (values: Object, helpers: FormikHelpers<any>) => Promise<void>;
}) => {
    const passwordSchema = yup.object().shape({
        password_confirm: yup.string().required("Password confirm is required"),
    });
    schema = schema.concat(passwordSchema);

    const [isEditing, setIsEditing] = useState(false);
    const titledField = uppercaseFirstLetter(field);

    if (isEditing)
        return (
            <Formik
                validationSchema={schema}
                initialValues={{
                    [field]: defaultValue,
                    password_confirm: "",
                }}
                onSubmit={onSubmit}
            >
                {({
                    handleSubmit,
                    handleChange,
                    values,
                    errors,
                    isSubmitting,
                    status,
                }) => (
                    <Form noValidate onSubmit={handleSubmit}>
                        <Card border="secondary">
                            <Form.Group>
                                <Form.Label>{titledField}</Form.Label>
                                <Form.Control
                                    aria-label={field}
                                    name={field}
                                    value={values[field]}
                                    onChange={handleChange}
                                    isInvalid={Boolean(errors[field])}
                                />

                                <Form.Control.Feedback type="invalid">
                                    {errors[field]}
                                </Form.Control.Feedback>
                            </Form.Group>

                            <PasswordConfirmField />

                            <SubmitRow
                                disableSubmitOn={
                                    errors[field] ||
                                    values[field] == field ||
                                    isSubmitting
                                }
                                onCancel={() => setIsEditing(false)}
                            />
                            <span className="text-invalid">{status}</span>
                        </Card>
                    </Form>
                )}
            </Formik>
        );

    return (
        <Form.Group>
            <Form.Label>{titledField}</Form.Label>
            <InputGroup>
                <Form.Control
                    aria-label={field}
                    value={defaultValue}
                    disabled
                />

                <FieldToggler
                    onToggle={() => setIsEditing(true)}
                    error={editingError}
                />
            </InputGroup>
        </Form.Group>
    );
};
export default PasswordField;
