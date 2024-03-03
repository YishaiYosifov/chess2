"use client";

import { BsPersonFill } from "react-icons/bs";
import { Form } from "react-bootstrap";

import { Formik, FormikHelpers } from "formik";
import { useRouter } from "next/navigation";
import { useContext } from "react";
import * as yup from "yup";

import constants from "@/lib/constants";
import { authApi } from "@/lib/apis";

import {
    FormInput,
    FormikField,
    PasswordInput,
    SubmitButton,
} from "../form/FormElements";
import { AuthContext, useAuthedProfile } from "../contexts/AuthContext";
import FormField from "../form/FormField";

export interface LoginFormValues {
    username: string;
    password: string;
}

const loginSchema = yup.object({
    username: yup.string().required(),
    password: yup.string().required(),
});

const LoginForm = () => {
    const { setHasAuthCookies } = useContext(AuthContext);
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
        } catch (err: any) {
            switch (err?.response?.status) {
                case 401:
                    setStatus("Wrong username / password");
                    break;
                default:
                    setStatus(constants.GENERIC_ERROR);
                    throw err;
            }
            return;
        }

        localStorage.setItem(
            constants.LAST_LOGIN_LOCAL_STORAGE,
            new Date().toUTCString()
        );
        setHasAuthCookies(true);
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
            {({ handleSubmit, status }) => (
                <Form
                    data-testid="loginForm"
                    aria-label="signup form"
                    noValidate
                    onSubmit={handleSubmit}
                >
                    <FormField hasValidation>
                        <FormikField
                            asInput={FormInput}
                            name="username"
                            placeholder="Username"
                            icon={<BsPersonFill />}
                        />
                    </FormField>

                    <FormField hasValidation>
                        <FormikField
                            asInput={PasswordInput}
                            name="password"
                            placeholder="Password"
                        />
                    </FormField>

                    <SubmitButton variant="secondary">Log In</SubmitButton>

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
