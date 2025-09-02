import { redirect } from "next/navigation";
import { cookies } from "next/headers";
import Image from "next/image";
import React from "react";

import discordIcon from "@public/assets/oauth/discord.svg";
import googleIcon from "@public/assets/oauth/google.svg";
import LogoText from "@public/assets/logo-text.svg";

import AuthPageImage from "@/features/auth/components/AuthPageImage";
import OAuthButton from "@/features/auth/components/OAuthButton";
import constants, { OAuthProvider } from "@/lib/constants";

export const metadata = { title: "Login - Chess 2" };

export default async function RegisterPage() {
    const cookieStore = await cookies();
    if (cookieStore.has(constants.COOKIES.IS_LOGGED_IN)) redirect("/");

    return (
        <div className="grid max-h-screen w-full justify-items-center md:grid-cols-[1.2fr_1.5fr]">
            <section className="m-auto flex max-h-full max-w-2xl flex-col items-center gap-10 overflow-auto p-10">
                <Image src={LogoText} alt="logo" className="h-auto w-auto" />

                <div className="flex w-full flex-col gap-3">
                    <OAuthButton
                        oauthProvider={OAuthProvider.GOOGLE}
                        icon={<Image src={googleIcon} alt="Google Icon" />}
                        loginText="Continue with Google"
                    />
                    <OAuthButton
                        oauthProvider={OAuthProvider.DISCORD}
                        icon={<Image src={discordIcon} alt="Discord Icon" />}
                        loginText="Continue with Discord"
                    />
                </div>
            </section>
            <AuthPageImage />
        </div>
    );
}
