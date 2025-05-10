"use client";

import { useRouter } from "next/navigation";

import constants from "@/lib/constants";
import Button from "../helpers/Button";

enum OAuthProvider {
    GOOGLE = "google",
}

const LoginForm = () => {
    const router = useRouter();

    function oAuthLogin(provider: OAuthProvider) {
        const url = new URL(constants.PATHS.OAUTH);
        url.pathname += provider;
        url.searchParams.append("returnUrl", window.location.origin);

        router.push(url.toString());
    }

    return (
        <Button onClick={async () => oAuthLogin(OAuthProvider.GOOGLE)}>
            Google
        </Button>
    );
};
export default LoginForm;
