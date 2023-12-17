"use client";

import { Form, Button, InputGroup } from "react-bootstrap";
import { BsPersonFill, BsEnvelopeFill } from "react-icons/bs";

import { useRouter } from "next/navigation";
import * as yup from "yup";

import { signup } from "@/lib/utils/authUtils";

import { Formik, FormikHelpers } from "formik";

import PasswordField from "../PasswordField";
import { FormikField } from "../FormField";

export interface SignupFormValues {
    username: string;
    email: string;
    password: string;
}

const SignupForm = () => {
    const router = useRouter();

    async function onSubmit(
        values: SignupFormValues,
        { setErrors, setStatus }: FormikHelpers<SignupFormValues>
    ) {
        const response = await signup(
            values.username,
            values.email,
            values.password
        );

        if (!response) {
            setStatus("Something went wrong.");
            return;
        } else if (response.ok) {
            router.push("/login");
            return;
        }

        const data = await response.json();

        switch (response.status) {
            case 409:
                setErrors(data.detail);
                break;
            default:
                console.error(data);
                setStatus("Something went wrong.");
                break;
        }
    }

    const schema = yup.object().shape({
        username: yup.string().username(),
        email: yup.string().email(),
        password: yup.string().password(),
    });

    return (
        <Formik
            validationSchema={schema}
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
                    <FormikField fieldName="username" placeholder="username">
                        <InputGroup.Text>
                            <BsPersonFill />
                        </InputGroup.Text>
                    </FormikField>

                    <FormikField
                        fieldName="email"
                        placeholder="email"
                        type="email"
                    >
                        <InputGroup.Text>
                            <BsEnvelopeFill />
                        </InputGroup.Text>
                    </FormikField>

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
