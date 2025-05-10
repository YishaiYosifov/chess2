"use client";

import { useRouter } from "next/navigation";
import Image from "next/image";
import React from "react";

import constants, { OAuthProvider } from "@/lib/constants";
import googleIcon from "@public/assets/oauth/google.svg";
import appleIcon from "@public/assets/oauth/apple.svg";

import LogoText from "@public/assets/logo-text.svg";
import AuthPageImage from "./AuthPageImage";
import Button from "../helpers/Button";
import OAuthButton from "./OAuthButton";

const AuthPage = () => {
    return (
        <div className="grid w-full justify-items-center md:grid-cols-[1.2fr_1.5fr]">
            <section className="flex max-w-3xl flex-col items-center justify-center gap-10 p-10">
                <Image src={LogoText} alt="logo" className="h-auto w-auto" />

                <div className="flex max-w-2xl flex-col gap-3">
                    <OAuthButton
                        oauthProvider={OAuthProvider.GOOGLE}
                        icon={<Image src={googleIcon} alt="Google Icon" />}
                        loginText="Continue with Google"
                    />

                    <OAuthButton
                        oauthProvider={OAuthProvider.APPLE}
                        icon={<Image src={appleIcon} alt="Apple Icon" />}
                        loginText="Continue with Apple"
                    />
                </div>
            </section>
            <AuthPageImage />
        </div>
    );
};
export default AuthPage;
