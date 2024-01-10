"use client";

import { Form, Button, InputGroup } from "react-bootstrap";
import { BsPersonFill, BsEnvelopeFill } from "react-icons/bs";

import { useRouter } from "next/navigation";
import * as yup from "yup";

import { Formik, FormikHelpers } from "formik";

import { usernameSchema, emailSchema, passwordSchema } from "@/lib/validation";
import { ResponseError } from "@/client";
import { authApi } from "@/lib/apis";

import { FormikInput } from "../FormikElements";
import PasswordField from "../PasswordField";
import FormField from "../FormField";

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

    async function onSubmit(
        values: SignupFormValues,
        { setErrors, setStatus }: FormikHelpers<SignupFormValues>
    ) {
        try {
            await authApi.signup({
                userIn: {
                    username: values.username,
                    email: values.email,
                    password: values.password,
                },
            });
        } catch (err) {
            if (!(err instanceof ResponseError)) {
                setStatus("Something went wrong.");
                return;
            }

            const data = await err.response.json();
            switch (err.response.status) {
                case 409:
                    setErrors(data.detail);
                    break;
                default:
                    console.error(data);
                    setStatus("Something went wrong.");
                    break;
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
            {({ handleSubmit, isSubmitting, status }) => (
                <Form
                    data-testid="signupForm"
                    aria-label="signup form"
                    noValidate
                    onSubmit={handleSubmit}
                >
                    <FormField hasValidation>
                        <FormikInput
                            fieldName="username"
                            placeholder="username"
                            icon={<BsPersonFill />}
                        />
                    </FormField>

                    <FormField hasValidation>
                        <FormikInput
                            fieldName="email"
                            placeholder="email"
                            type="email"
                            icon={<BsEnvelopeFill />}
                        />
                    </FormField>

                    <PasswordField />

                    <Button
                        type="submit"
                        variant="secondary"
                        disabled={isSubmitting}
                        data-testid="submitForm"
                    >
                        Sign Up
                    </Button>
                    {status && <span className="text-invalid">{status}</span>}
                </Form>
            )}
        </Formik>
    );
};
export default SignupForm;
