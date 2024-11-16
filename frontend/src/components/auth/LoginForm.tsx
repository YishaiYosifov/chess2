"use client";

import { FormikHelpers } from "formik";
import { useRouter } from "next/navigation";
import { useContext } from "react";
import Image from "next/image";
import Link from "next/link";
import * as yup from "yup";

import constants from "@/lib/constants";
import { authApi } from "@/lib/apis";

import { AuthContext } from "@/contexts/authContext";
import LogoText from "@public/assets/logo-text.svg";

export interface LoginFormValues {
    username: string;
    password: string;
}

const loginSchema = yup.object({
    username: yup.string().required(),
    password: yup.string().required(),
});

const LoginForm = () => {
    const { setHasAuthCookies } = useContext(AuthContext);
    const router = useRouter();

    async function onSubmit(
        values: LoginFormValues,
        { setStatus }: FormikHelpers<LoginFormValues>,
    ) {
        try {
            await authApi.login({
                username: values.username,
                password: values.password,
            });
        } catch (err: any) {
            switch (err?.response?.status) {
                case 401:
                    setStatus("Wrong username / password");
                    break;
                default:
                    setStatus(constants.GENERIC_ERROR);
                    throw err;
            }
            return;
        }

        localStorage.setItem(
            constants.LAST_LOGIN_LOCAL_STORAGE,
            new Date().toUTCString(),
        );
        setHasAuthCookies(true);
        router.replace("/");
    }

    return (
        <section className="flex max-w-5xl flex-col items-center justify-center gap-10 px-10">
            <Image src={LogoText} alt="logo" />

            <form
                data-testid="loginForm"
                aria-label="signup form"
                className="flex w-4/5 flex-col gap-5 text-center"
            >
                <div className="flex flex-col gap-3 text-black">
                    <input
                        className="w-full rounded-md p-1"
                        placeholder="Username"
                    />
                    <input
                        className="w-full rounded-md p-1"
                        placeholder="Email"
                        type="email"
                    />
                    <input
                        className="w-full rounded-md p-1"
                        placeholder="Password"
                        type="password"
                    />
                </div>

                <button
                    className="rounded-md bg-cta p-2 text-3xl"
                    type="submit"
                >
                    Sign Up
                </button>
                <span data-testid="signupLink">
                    Don&#39;t have an account? Click{" "}
                    {
                        <Link href="/signup" className="text-link">
                            here to sign up
                        </Link>
                    }
                </span>
            </form>
        </section>
    );
};
export default LoginForm;
