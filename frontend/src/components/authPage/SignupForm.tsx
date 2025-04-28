"use client";

import { getCountryForTimezone } from "countries-and-timezones";
import { useRouter } from "next/navigation";
import { useMemo } from "react";
import * as yup from "yup";

import { Form, Formik, FormikHelpers } from "formik";
import Link from "next/link";

import { usernameSchema, emailSchema, passwordSchema } from "@/lib/validation";
import { ResponseError } from "@/lib/apiClient/models";
import { authApi } from "@/lib/apiClient/client";
import constants from "@/lib/constants";

import FormikSubmitButton from "../helpers/FormikSubmitButton";
import Input, { PasswordInput } from "../helpers/Input";
import FormikField from "../helpers/FormikField";

export interface SignupFormValues {
    username: string;
    email: string;
    password: string;
}

const signupSchema = yup.object({
    username: usernameSchema,
    email: emailSchema,
    password: passwordSchema,
});

const SignupForm = () => {
    const router = useRouter();
    const countryCode = useMemo(
        () =>
            getCountryForTimezone(
                Intl.DateTimeFormat().resolvedOptions().timeZone,
            )?.id,
        [],
    );

    async function onSubmit(
        values: SignupFormValues,
        { setErrors, setStatus }: FormikHelpers<SignupFormValues>,
    ) {
        try {
            await authApi.signup({
                userName: values.username,
                email: values.email,
                password: values.password,
                countryCode,
            });
        } catch (err) {
            if (!(err instanceof ResponseError)) {
                setStatus(constants.GENERIC_ERROR);
                throw err;
            }

            switch (err.status) {
                case 409:
                    setErrors(err.toFormik());
                    break;
                default:
                    setStatus(constants.GENERIC_ERROR);
                    throw err;
            }
            return;
        }

        router.push("/login");
    }

    return (
        <Formik
            validationSchema={signupSchema}
            onSubmit={onSubmit}
            initialValues={{
                username: "",
                email: "",
                password: "",
            }}
        >
            <Form
                data-testid="signupForm"
                aria-label="signup form"
                className="flex w-4/5 flex-col gap-7"
            >
                <div className="flex flex-col gap-3">
                    <FormikField
                        asInput={Input}
                        label="Username"
                        name="username"
                        placeholder="Enter a username"
                    />
                    <FormikField
                        asInput={Input}
                        label="Email"
                        name="email"
                        placeholder="Enter an email"
                        type="email"
                    />
                    <FormikField
                        asInput={PasswordInput}
                        name="password"
                        label="Password"
                    />
                </div>

                <FormikSubmitButton>Sign Up</FormikSubmitButton>
                <span data-testid="loginLink" className="text-center">
                    Already have an account? Click{" "}
                    {
                        <Link href="/login" className="text-link">
                            here to log in
                        </Link>
                    }
                </span>
            </Form>
        </Formik>
    );
};
export default SignupForm;
