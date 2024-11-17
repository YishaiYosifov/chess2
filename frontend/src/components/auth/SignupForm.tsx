"use client";

import { Form, Formik, FormikHelpers } from "formik";
import { useRouter } from "next/navigation";
import { useMemo } from "react";
import * as yup from "yup";

import { usernameSchema, emailSchema, passwordSchema } from "@/lib/validation";
import { getCountryFromUserTimezone } from "@/lib/utils/geolocation";
import { ResponseError } from "@/lib/models";
import constants from "@/lib/constants";
import { authApi } from "@/lib/apis";

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
    const countryCode = useMemo(getCountryFromUserTimezone, []);

    async function onSubmit(
        values: SignupFormValues,
        { setErrors, setStatus }: FormikHelpers<SignupFormValues>,
    ) {
        try {
            await authApi.signup({
                username: values.username,
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
                className="flex w-4/5 flex-col gap-5"
            >
                <div className="flex flex-col gap-3">
                    <FormikField
                        asInput={Input}
                        name="username"
                        placeholder="username"
                    />
                    <FormikField
                        asInput={Input}
                        name="email"
                        placeholder="email"
                        type="email"
                    />
                    <FormikField asInput={PasswordInput} name="password" />
                </div>

                <FormikSubmitButton>Sign Up</FormikSubmitButton>
            </Form>
        </Formik>
    );
};
export default SignupForm;
