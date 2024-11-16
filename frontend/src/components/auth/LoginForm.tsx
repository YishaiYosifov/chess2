"use client";

import { ErrorMessage, Field, Form, Formik, FormikHelpers } from "formik";
import { useRouter } from "next/navigation";
import { useContext } from "react";
import Link from "next/link";
import * as yup from "yup";

import constants from "@/lib/constants";
import { authApi } from "@/lib/apis";

import { AuthContext } from "@/contexts/authContext";

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
        console.log(123);
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
                className="flex w-4/5 flex-col gap-5"
            >
                <div className="flex flex-col gap-3 text-black">
                    <span className="text-error">
                        <ErrorMessage name="usernameOrEmail" />
                    </span>
                    <Field
                        className="w-full rounded-md p-1"
                        placeholder="Username or Email"
                        name="usernameOrEmail"
                    />
                    <span className="text-error">
                        <ErrorMessage name="password" />
                    </span>
                    <Field
                        className="w-full rounded-md p-1"
                        placeholder="Password"
                        name="password"
                        type="password"
                    />
                </div>

                <button
                    className="rounded-md bg-cta p-2 text-3xl"
                    type="submit"
                >
                    Log In
                </button>
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
