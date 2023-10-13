"use client";

import Image from "next/image";

import logoText from "@/public/assets/logo-text.svg";
import anarchy from "@/public/assets/anarchy.jpg";
import styles from "./authPage.module.scss";

import SignupForm from "./SignupForm";
import LoginForm from "./LoginForm";
import { Card } from "react-bootstrap";

/**
 * Both the login and signup page are the same but with a different form,
 * so this component groups them together.
 *
 * This component should only be imported to the login/signup pages.
 *
 * @param login - whether to render the login form
 * @param signup - whether to render the signup form
 */
const AuthPage = ({
    login,
    signup,
}: { login: true; signup?: false } | { login?: false; signup: true }) => {
    return (
        <div className={styles.container}>
            <div className={styles.form}>
                <Image
                    src={logoText}
                    alt="logo"
                    className={styles["logo-text"]}
                />
                <h1>{login ? "Login" : "Signup"}</h1>

                <Card className={styles.card}>
                    {login ? <LoginForm /> : <SignupForm />}
                </Card>
            </div>
            <div className={styles["side-image-container"]}>
                <svg width="100%" height="100%">
                    <pattern
                        id="chessboard-pattern"
                        width="80"
                        height="80"
                        patternUnits="userSpaceOnUse"
                    >
                        <rect width="40" height="40" fill="black" />
                        <rect
                            x="40"
                            y="40"
                            width="40"
                            height="40"
                            fill="black"
                        />
                    </pattern>

                    <rect
                        width="100%"
                        height="100%"
                        fill="url(#chessboard-pattern)"
                    />
                </svg>
            </div>
        </div>
    );
};
export default AuthPage;
