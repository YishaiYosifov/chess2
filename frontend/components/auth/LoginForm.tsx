"use client";

import { InputGroup, Form, Button } from "react-bootstrap";
import {
    BsEyeFill,
    BsEyeSlashFill,
    BsPersonFill,
    BsUnlockFill,
} from "react-icons/bs";

import * as yup from "yup";

import { useRef, useState, SyntheticEvent } from "react";
import { useRouter } from "next/navigation";

import { login } from "@/lib/utils/auth";
import { FormikField } from "../FormField";
import { Formik, FormikHelpers } from "formik";

interface LoginFormValues {
    username: string;
    password: string;
}

const LoginForm = () => {
    const router = useRouter();

    const [isShowingPassword, setIsShowingPassword] = useState(false);

    async function onSubmit(
        values: LoginFormValues,
        { setErrors, setStatus }: FormikHelpers<LoginFormValues>
    ) {
        const { status, response } = await login(
            values.username,
            values.password
        );
        const json = await response.json();

        switch (json.status) {
            case "success":
                router.replace("/");
                break;
            case "fail":
                console.log(json);
                setErrors(json.data);
                break;
            default:
                setStatus("Something went wrong.");
                console.error(json);
                break;
        }
    }
    const EyeToggle = isShowingPassword ? BsEyeFill : BsEyeSlashFill;

    return (
        <Formik
            onSubmit={onSubmit}
            initialValues={{ username: "luka", password: "securePassword123" }}
        >
            {({ handleSubmit, isSubmitting, status }) => (
                <Form noValidate onSubmit={handleSubmit}>
                    <FormikField fieldName="username" placeholder="Username">
                        <InputGroup.Text>
                            <BsPersonFill />
                        </InputGroup.Text>
                    </FormikField>

                    <FormikField
                        fieldName="password"
                        placeholder="Password"
                        type={isShowingPassword ? "text" : "password"}
                    >
                        <InputGroup.Text>
                            <BsUnlockFill />
                            <EyeToggle
                                onClick={() =>
                                    setIsShowingPassword(!isShowingPassword)
                                }
                                role="button"
                            />
                        </InputGroup.Text>
                    </FormikField>

                    <Button
                        type="submit"
                        variant="secondary"
                        disabled={isSubmitting}
                    >
                        Log In
                    </Button>

                    {status && <span className="text-invalid">{status}</span>}
                </Form>
            )}
        </Formik>
    );
};
export default LoginForm;
