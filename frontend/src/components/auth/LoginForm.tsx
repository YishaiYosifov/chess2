"use client";

import { InputGroup, Form, Button } from "react-bootstrap";
import { BsPersonFill } from "react-icons/bs";

import { Formik, FormikHelpers } from "formik";
import { useRouter } from "next/navigation";
import * as yup from "yup";

import { setIsAuthed } from "@/zustand/store";
import { ResponseError } from "@/client";
import { authApi } from "@/lib/apis";

import { FormikInput } from "../FormikElements";
import PasswordField from "../PasswordField";
import FormField from "../FormField";

export interface LoginFormValues {
    username: string;
    password: string;
}

const loginSchema = yup.object({
    username: yup.string().required(),
    password: yup.string().required(),
});

const LoginForm = () => {
    const router = useRouter();

    async function onSubmit(
        values: LoginFormValues,
        { setStatus }: FormikHelpers<LoginFormValues>
    ) {
        try {
            await authApi.login({
                username: values.username,
                password: values.password,
            });
        } catch (err) {
            if (!(err instanceof ResponseError)) {
                setStatus("Something went wrong.");
                return;
            }

            switch (err.response.status) {
                case 401:
                    setStatus("Wrong username / password");
                    break;
                default:
                    setStatus("Something went wrong.");
                    console.error(
                        err.response.status,
                        await err.response.text()
                    );
                    break;
            }
            return;
        }

        setIsAuthed(true);
        router.replace("/");
    }

    return (
        <Formik
            onSubmit={onSubmit}
            validationSchema={loginSchema}
            initialValues={{
                username: "",
                password: "",
            }}
        >
            {({ handleSubmit, isSubmitting, status }) => (
                <Form
                    data-testid="loginForm"
                    aria-label="signup form"
                    noValidate
                    onSubmit={handleSubmit}
                >
                    <FormField hasValidation>
                        <FormikInput
                            fieldName="username"
                            placeholder="Username"
                            icon={<BsPersonFill />}
                        />
                    </FormField>

                    <PasswordField />

                    <Button
                        type="submit"
                        variant="secondary"
                        disabled={isSubmitting}
                        data-testid="submitForm"
                    >
                        Log In
                    </Button>

                    {status && (
                        <span data-testid="formStatus" className="text-invalid">
                            {status}
                        </span>
                    )}
                </Form>
            )}
        </Formik>
    );
};
export default LoginForm;
