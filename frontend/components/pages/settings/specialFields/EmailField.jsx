import { Card, Form, InputGroup } from "react-bootstrap";

import { useStore } from "@/app/store";
import { useState } from "react";

import * as yup from "yup";
import { Formik } from "formik";
import PasswordConfirmField from "./helpers/PasswordConfirmField";
import FieldToggler from "./helpers/FieldToggler";
import SubmitRow from "../SubmitRow";

const EmailField = () => {
    const { email, emailLastChanged } = useStore.use.profile();
    const [isEditing, setIsEditing] = useState(false);

    const schema = yup.object().shape({
        email: yup.string().email(),
        password_confirm: yup.string().required("Password confirm is required"),
    });

    const timeSinceEmailChange =
        (new Date() - new Date(emailLastChanged)) / 1000;
    const didEmailChangeRecently =
        timeSinceEmailChange < process.env.NEXT_PUBLIC_EMAIL_EDIT_EVERY;

    return (
        <>
            {isEditing ? (
                <Formik
                    validationSchema={schema}
                    initialValues={{
                        email: email,
                        password_confirm: "",
                    }}
                    //onSubmit={updateSettings}
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
                                    <Form.Label>Email</Form.Label>
                                    <Form.Control
                                        aria-label="email"
                                        name="email"
                                        value={values.email}
                                        onChange={handleChange}
                                        isInvalid={errors.email}
                                    />

                                    <Form.Control.Feedback type="invalid">
                                        {errors.email}
                                    </Form.Control.Feedback>
                                </Form.Group>

                                <PasswordConfirmField />

                                <SubmitRow
                                    disableSubmitOn={
                                        errors.email ||
                                        values.email == email ||
                                        isSubmitting
                                    }
                                    onCancel={() => setIsEditing(false)}
                                />
                                <span className="text-invalid">{status}</span>
                            </Card>
                        </Form>
                    )}
                </Formik>
            ) : (
                <Form.Group>
                    <Form.Label>Email</Form.Label>
                    <InputGroup>
                        <Form.Control
                            aria-label="email"
                            value={email}
                            disabled
                        />

                        <FieldToggler
                            onToggle={() => setIsEditing(true)}
                            error={
                                didEmailChangeRecently &&
                                "Email changed too recently"
                            }
                        />
                    </InputGroup>
                </Form.Group>
            )}
        </>
    );
};
export default EmailField;
