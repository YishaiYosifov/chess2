"use client";

import { Form, Formik, FormikHelpers } from "formik";
import { useRouter } from "next/navigation";
import { useContext } from "react";
import Link from "next/link";
import * as yup from "yup";

import constants from "@/lib/constants";
import { signin } from "@/lib/apiClient";

import FormikSubmitButton from "../helpers/FormikSubmitButton";
import Input, { PasswordInput } from "../helpers/Input";
import { AuthContext } from "@/contexts/authContext";
import FormikField from "../helpers/FormikField";
import { signIn } from "next-auth/react";

export interface LoginFormValues {
    usernameOrEmail: string;
    password: string;
}

const loginSchema = yup.object({
    usernameOrEmail: yup
        .string()
        .required("You must provide a username or an email"),
    password: yup.string().required("You must provide a password"),
});

const LoginForm = () => {
    const { setHasAuthCookies } = useContext(AuthContext);
    const router = useRouter();

    async function onSubmit(
        values: LoginFormValues,
        { setStatus }: FormikHelpers<LoginFormValues>,
    ): Promise<void> {
        const response = await signIn("credentials", {
            usernameOrEmail: values.usernameOrEmail,
            password: values.password,
            redirect: false,
        });
        if (!response?.ok) {
            switch (response?.status) {
                case 401:
                    setStatus("Wrong username / email / password");
                    break;
                default:
                    setStatus(constants.GENERIC_ERROR);
            }
            return;
        }

        localStorage.setItem(
            constants.LAST_LOGIN_LOCAL_STORAGE,
            new Date().toUTCString(),
        );
        setHasAuthCookies(true);
        router.replace("/");
    }

    return (
        <Formik
            initialValues={{ usernameOrEmail: "", password: "" }}
            validationSchema={loginSchema}
            onSubmit={onSubmit}
        >
            <Form
                data-testid="loginForm"
                aria-label="signup form"
                className="flex w-4/5 flex-col gap-7"
            >
                <div className="flex flex-col gap-3">
                    <FormikField
                        asInput={Input}
                        label="Username / Email"
                        placeholder="Enter your username or email"
                        name="usernameOrEmail"
                    />
                    <FormikField
                        asInput={PasswordInput}
                        label="Password"
                        name="password"
                    />
                </div>

                <FormikSubmitButton>Log In</FormikSubmitButton>

                <span data-testid="signupLink" className="text-center">
                    Don&#39;t have an account? Click{" "}
                    {
                        <Link href="/signup" className="text-link">
                            here to sign up
                        </Link>
                    }
                </span>
            </Form>
        </Formik>
    );
};
export default LoginForm;
