import { Card, Form, InputGroup } from "react-bootstrap";
import { Formik } from "formik";

import { useState } from "react";
import * as yup from "yup";

import { useStore } from "@/app/store";

import PasswordConfirmField from "./helpers/PasswordConfirmField";
import FieldToggler from "./helpers/FieldToggler";
import SubmitRow from "../SubmitRow";

const UsernameField = () => {
    const { username, usernameLastChanged } = useStore.use.profile();
    const [isEditing, setIsEditing] = useState(false);

    const schema = yup.object().shape({
        username: yup.string().username(),
        password_confirm: yup.string().required("Password confirm is required"),
    });

    const timeSinceUsernameChange =
        (new Date() - new Date(usernameLastChanged)) / 1000;
    const didUsernameChangeRecently =
        timeSinceUsernameChange < process.env.NEXT_PUBLIC_USERNAME_EDIT_EVERY;

    return (
        <>
            {isEditing ? (
                <Formik
                    validationSchema={schema}
                    initialValues={{
                        username: username,
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
                                    <Form.Label>Username</Form.Label>
                                    <Form.Control
                                        aria-label="username"
                                        name="username"
                                        value={values.username}
                                        onChange={handleChange}
                                        isInvalid={errors.username}
                                    />

                                    <Form.Control.Feedback type="invalid">
                                        {errors.username}
                                    </Form.Control.Feedback>
                                </Form.Group>

                                <PasswordConfirmField />

                                <SubmitRow
                                    disableSubmitOn={
                                        errors.username ||
                                        values.username == username ||
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
                    <Form.Label>Username</Form.Label>
                    <InputGroup>
                        <Form.Control
                            aria-label="username"
                            value={username}
                            disabled
                        />

                        <FieldToggler
                            onToggle={() => setIsEditing(true)}
                            error={
                                didUsernameChangeRecently &&
                                "Username changed too recently"
                            }
                        />
                    </InputGroup>
                </Form.Group>
            )}
        </>
    );
};
export default UsernameField;
