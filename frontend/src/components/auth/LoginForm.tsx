"use client";

import { InputGroup, Form, Button } from "react-bootstrap";
import { BsPersonFill } from "react-icons/bs";

import { Formik, FormikHelpers } from "formik";
import { useRouter } from "next/navigation";

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
        { setErrors, setStatus }: FormikHelpers<LoginFormValues>
    ) {
        const { status, data } = await login(values.username, values.password);

        switch (status) {
            case 200:
                router.replace("/");
                break;
            case 401:
                setStatus("Wrong username / password");
                break;
            default:
                setStatus("Something went wrong.");
                console.error(status, data);
                break;
        }
    }

    return (
        <div data-testid="loginForm">
            <Formik
                onSubmit={onSubmit}
                initialValues={{
                    username: "luka",
                    password: "securePassword123",
                }}
            >
                {({ handleSubmit, isSubmitting, status }) => (
                    <Form noValidate onSubmit={handleSubmit}>
                        <FormikField
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
                        >
                            Log In
                        </Button>

                        {status && (
                            <span className="text-invalid">{status}</span>
                        )}
                    </Form>
                )}
            </Formik>
        </div>
    );
};
export default LoginForm;
