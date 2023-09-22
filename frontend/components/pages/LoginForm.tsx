"use client";

import { InputGroup, Form, Button, Col } from "react-bootstrap";
import {
    BsEyeFill,
    BsEyeSlashFill,
    BsPersonFill,
    BsUnlockFill,
} from "react-icons/bs";
import Image from "next/image";
import Link from "next/link";

import { useRef, useState, SyntheticEvent } from "react";
import { useRouter } from "next/navigation";

import { login } from "@/lib/utils/auth";

const LoginForm = () => {
    const router = useRouter();

    const [isShowingPassword, setIsShowingPassword] = useState(false);
    const [status, setStatus] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const selectorInput = useRef<HTMLInputElement>(null);
    const passwordInput = useRef<HTMLInputElement>(null);

    async function onSubmit(event: SyntheticEvent<HTMLFormElement>) {
        event.preventDefault();
        setIsLoading(true);

        setStatus("");
        const { status, response } = await login(
            selectorInput.current?.value,
            passwordInput.current?.value
        );
        const { msg } = await response.json();

        switch (status) {
            case 200:
                router.replace("/");
                break;
            case 401:
                setStatus(msg);
                break;
            default:
                setStatus("Something went wrong.");
                console.error(msg);
                break;
        }
        setIsLoading(false);
    }
    const EyeToggle = isShowingPassword ? BsEyeFill : BsEyeSlashFill;

    return (
        <Form noValidate onSubmit={onSubmit}>
            <InputGroup className="mb-1">
                <Form.Control
                    placeholder="Username / Email"
                    aria-label="Username / Email"
                    value="luka"
                    ref={selectorInput}
                    onChange={() => {}}
                />
                <InputGroup.Text>
                    <BsPersonFill />
                </InputGroup.Text>
            </InputGroup>

            <InputGroup className="mt-3 mb-1">
                <Form.Control
                    placeholder="Password"
                    pattern="[A-Za-z]{3}"
                    type={isShowingPassword ? "text" : "password"}
                    aria-label="Password"
                    ref={passwordInput}
                    value="securePassword123"
                    onChange={() => {}}
                />
                <InputGroup.Text>
                    <BsUnlockFill />
                    <EyeToggle
                        onClick={() => setIsShowingPassword(!isShowingPassword)}
                        role="button"
                    />
                </InputGroup.Text>
            </InputGroup>

            <Col sm={12}>
                <Button
                    type="submit"
                    variant="secondary"
                    className="text-white mt-3 w-100"
                    disabled={isLoading}
                >
                    Log In
                </Button>
            </Col>
            <span className="text-invalid">{status}</span>

            <hr></hr>
            <Col sm={12}>
                <Button as="a" variant="primary" className="w-100">
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
                Don't have an account? click{" "}
                <Link href="/signup">here to sign up</Link>
            </p>
        </Form>
    );
};
export default LoginForm;
