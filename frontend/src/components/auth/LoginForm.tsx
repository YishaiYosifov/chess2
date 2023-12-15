"use client";

import { InputGroup, Form, Button } from "react-bootstrap";
import { BsPersonFill } from "react-icons/bs";

import { Formik, FormikHelpers } from "formik";
import { useRouter } from "next/navigation";
import * as yup from "yup";

import { login } from "@/lib/utils/authUtils";
import PasswordField from "../PasswordField";
import { FormikField } from "../FormField";

interface LoginFormValues {
    username: string;
    password: string;
}

const LoginForm = () => {
    const router = useRouter();

    async function onSubmit(
        values: LoginFormValues,
        { setStatus }: FormikHelpers<LoginFormValues>
    ) {
        const response = await login(values.username, values.password);

        if (!response) {
            setStatus("Something went wrong.");
            return;
        }

        switch (response.status) {
            case 200:
                router.replace("/");
                break;
            case 401:
                setStatus("Wrong username / password");
                break;
            default:
                setStatus("Something went wrong.");
                console.error(response.status, await response.text());
                break;
        }
    }

    const schema = yup.object().shape({
        username: yup.string().required(),
        password: yup.string().required(),
    });

    return (
        <div data-testid="loginForm">
            <Formik
                onSubmit={onSubmit}
                validationSchema={schema}
                initialValues={{
                    username: "",
                    password: "",
                }}
            >
                {({ handleSubmit, isSubmitting, status }) => (
                    <Form noValidate onSubmit={handleSubmit}>
                        <FormikField
                            data-testid="usernameField"
                            fieldName="username"
                            placeholder="Username"
                        >
                            <InputGroup.Text>
                                <BsPersonFill />
                            </InputGroup.Text>
                        </FormikField>

                        <PasswordField />

                        <Button
                            type="submit"
                            variant="secondary"
                            disabled={isSubmitting}
                            data-testid="submitLoginForm"
                        >
                            Log In
                        </Button>

                        {status && (
                            <span
                                data-testid="formStatus"
                                className="text-invalid"
                            >
                                {status}
                            </span>
                        )}
                    </Form>
                )}
            </Formik>
        </div>
    );
};
export default LoginForm;
