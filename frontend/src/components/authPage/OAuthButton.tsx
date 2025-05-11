"use client";

import { useRouter } from "next/navigation";
import React from "react";

import constants, { OAuthProvider } from "@/lib/constants";
import Button from "../helpers/Button";
import clsx from "clsx";

const OAuthButton = ({
    className,
    oauthProvider,
    loginText,
    icon,
    ...props
}: {
    icon: React.ReactNode;
    oauthProvider: OAuthProvider;
    loginText: string;
} & React.ButtonHTMLAttributes<HTMLButtonElement>) => {
    const router = useRouter();

    function oAuthLogin() {
        const url = new URL(constants.PATHS.OAUTH);
        url.pathname += oauthProvider;
        url.searchParams.append("returnUrl", window.location.origin);

        router.push(url.toString());
    }

    return (
        <Button
            className={clsx(
                `flex cursor-pointer items-center justify-center gap-3 bg-white px-2 font-sans
                text-[20px] font-bold text-black`,
                className,
            )}
            onClick={oAuthLogin}
            {...props}
        >
            {icon}
            {loginText}
        </Button>
    );
};
export default OAuthButton;
