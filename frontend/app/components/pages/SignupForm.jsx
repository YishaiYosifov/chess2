"use client";

import {
    Col,
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
import Image from "next/image";
import Link from "next/link";

import { useRouter } from "next/navigation";
import { useState } from "react";
import * as yup from "yup";

import { apiRequest } from "@/app/utils/common";
import { Formik } from "formik";

const SignupForm = () => {
    const router = useRouter();

    const [isShowingPassword, setIsShowingPassword] = useState(false);
    const [generalError, setGeneralError] = useState("");

    async function onSubmit(form, helpers) {
        setGeneralError("");
        helpers.setSubmitting(true);
        const response = await apiRequest("/auth/signup", {
            json: form,
        });

        if (response.ok) {
            router.push("/login");
            return;
        }
        helpers.setSubmitting(false);

        switch (response.status) {
            case 400:
            case 401:
                helpers.setErrors((await response.json()).msg);
                break;
            default:
                console.error(await response.json());
                setGeneralError("Something went wrong.");
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
            {({ handleSubmit, handleChange, values, errors, isSubmitting }) => (
                <Form noValidate onSubmit={handleSubmit}>
                    <Form.Group>
                        <InputGroup hasValidation>
                            <Form.Control
                                placeholder="username"
                                aria-label="username"
                                name="username"
                                value={values.username}
                                onChange={handleChange}
                                isInvalid={errors.username}
                            />
                            <InputGroup.Text>
                                <BsPersonFill />
                            </InputGroup.Text>
                            <Form.Control.Feedback type="invalid">
                                {errors.username}
                            </Form.Control.Feedback>
                        </InputGroup>
                    </Form.Group>

                    <Form.Group className="mt-3 mb-1">
                        <InputGroup hasValidation>
                            <Form.Control
                                placeholder="Email"
                                aria-label="Email"
                                name="email"
                                type="email"
                                value={values.email}
                                onChange={handleChange}
                                isInvalid={errors.email}
                            />

                            <InputGroup.Text>
                                <BsEnvelopeFill />
                            </InputGroup.Text>

                            <Form.Control.Feedback type="invalid">
                                {errors.email}
                            </Form.Control.Feedback>
                        </InputGroup>
                    </Form.Group>

                    <Form.Group className="mt-3 mb-1">
                        <InputGroup hasValidation>
                            <Form.Control
                                placeholder="Password"
                                aria-label="password"
                                name="password"
                                type={isShowingPassword ? "text" : "password"}
                                value={values.password}
                                onChange={handleChange}
                                isInvalid={errors.password}
                            />

                            <InputGroup.Text>
                                <BsUnlockFill />

                                <OverlayTrigger
                                    overlay={
                                        <Tooltip>
                                            <b>
                                                Your password must be at least 8
                                                characters long, have both lower
                                                and upper case letters and
                                                include a number.
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

                            <Form.Control.Feedback type="invalid">
                                {errors.password}
                            </Form.Control.Feedback>
                        </InputGroup>
                    </Form.Group>

                    <Col sm={12}>
                        <Button
                            type="submit"
                            variant="secondary"
                            className="text-white mt-3 w-100"
                            disabled={isSubmitting}
                        >
                            Sign Up
                        </Button>
                    </Col>
                    <span className="text-invalid">{generalError}</span>

                    <hr />

                    <Col sm={12}>
                        <Button as="a" className="w-100" variant="primary">
                            <Image
                                src="/assets/google.webp"
                                alt="google logo"
                                width={30}
                                height={30}
                                className="me-2"
                            />
                            Continue with Google
                        </Button>
                    </Col>

                    <p className="text-center mt-3">
                        Already have an account? click{" "}
                        <Link href="/login">here to log in</Link>
                    </p>
                </Form>
            )}
        </Formik>
    );
};
export default SignupForm;
