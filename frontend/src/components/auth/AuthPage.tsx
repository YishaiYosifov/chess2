"use client";

import Image from "next/image";
import Link from "next/link";

import SignupForm from "./SignupForm";
import LoginForm from "./LoginForm";

export enum AuthPageType {
    Login,
    Signup,
}

/**
 * Both the login and signup page are the same but with a different form,
 * so this component groups them together.
 *
 * This component should only be imported to the login/signup pages.
 *
 * @param login - whether to render the login form
 * @param signup - whether to render the signup form
 */
const AuthPage = ({ pageType }: { pageType: AuthPageType }) => {
    return (
        <div className="">
            <div className={styles.form}>
                <Image
                    src="/assets/logo-text.svg"
                    width={300}
                    height={83}
                    alt="logo"
                    className={styles["logo-text"]}
                />
                <h1>{pageType == AuthPageType.Login ? "Login" : "Signup"}</h1>

                <div className={styles["form-container"]}>
                    {login ? <LoginForm /> : <SignupForm />}
                    <hr />

                    {login ? (
                        <span data-testid="signupLink">
                            Don&#39;t have an account? Click{" "}
                            {<Link href="/signup">here to sign up</Link>}
                        </span>
                    ) : (
                        <span data-testid="loginLink">
                            Already have an account? Click{" "}
                            {<Link href="/login">here to log in</Link>}
                        </span>
                    )}
                </div>
            </div>
            <div className={styles["side-image-container"]}>
                <Image
                    src="/assets/pieces-svg/knook-white.svg"
                    width={600}
                    height={600}
                    alt="knook"
                    draggable={false}
                />
            </div>
        </div>
    );
};
export default AuthPage;
