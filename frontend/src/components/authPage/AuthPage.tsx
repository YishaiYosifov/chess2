"use client";

import Image from "next/image";
import React from "react";

import LogoText from "@public/assets/logo-text.svg";
import AuthPageImage from "./AuthPageImage";

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
const AuthPage = ({ form }: { form: React.ReactNode }) => {
    return (
        <div className="grid h-[calc(100vh-72px)] justify-items-center md:grid-cols-[1fr_1.5fr]">
            <section className="flex max-w-5xl flex-col items-center justify-center gap-10 px-10">
                <Image src={LogoText} alt="logo" />
                {form}
            </section>
            <AuthPageImage />
        </div>
    );
};
export default AuthPage;
