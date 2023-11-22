"use client";

import {
    Form,
    Button,
    Tooltip,
    InputGroup,
    OverlayTrigger,
} from "react-bootstrap";
import {
    BsEyeFill,
    BsPersonFill,
    BsUnlockFill,
    BsEyeSlashFill,
    BsEnvelopeFill,
    BsQuestionCircleFill,
} from "react-icons/bs";

import { useRouter } from "next/navigation";
import { useState } from "react";
import * as yup from "yup";

import { apiRequest } from "@/lib/utils/common";
import { Formik, FormikHelpers } from "formik";
import { FormikField } from "../FormField";

interface SignupFormValues {
    username: string;
    email: string;
    password: string;
}

const SignupForm = () => {
    const router = useRouter();

    const [isShowingPassword, setIsShowingPassword] = useState(false);

    async function onSubmit(
        values: SignupFormValues,
        { setErrors, setSubmitting, setStatus }: FormikHelpers<SignupFormValues>
    ) {
        const response = await apiRequest("/auth/signup", {
            json: values,
        });

        if (response.ok) {
            router.push("/login");
            return;
        }
        setSubmitting(false);

        switch (response.status) {
            case 400:
            case 409:
            case 401:
                setErrors((await response.json()).data);
                break;
            default:
                console.error(await response.json());
                setStatus("Something went wrong.");
                break;
        }
    }

    const EyeToggle = isShowingPassword ? BsEyeFill : BsEyeSlashFill;

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
                username: "luka",
                email: "willigooden.uk@gmail.com",
                password: "securePassword123",
            }}
        >
            {({ handleSubmit, isSubmitting, status }) => (
                <Form noValidate onSubmit={handleSubmit}>
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

                    <FormikField
                        fieldName="password"
                        placeholder="password"
                        type={isShowingPassword ? "text" : "password"}
                    >
                        <InputGroup.Text>
                            <BsUnlockFill />

                            <OverlayTrigger
                                overlay={
                                    <Tooltip>
                                        <b>
                                            Your password must be at least 8
                                            characters long, have both lower and
                                            upper case letters and include a
                                            number.
                                        </b>
                                    </Tooltip>
                                }
                            >
                                <a>
                                    <BsQuestionCircleFill />
                                </a>
                            </OverlayTrigger>

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
                        Sign Up
                    </Button>
                    {status && <span className="text-invalid">{status}</span>}
                </Form>
            )}
        </Formik>
    );
};
export default SignupForm;
