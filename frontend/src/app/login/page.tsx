import LogoText from "@public/assets/logo-text.svg";
import Image from "next/image";

import AuthPageImage from "@/components/auth/AuthPageImage";
import SignupForm from "@/components/auth/SignupForm";
import LoginForm from "@/components/auth/LoginForm";
import withoutAuth from "@/hocs/withoutAuth";

export const metadata = { title: "Login - Chess 2" };

const LoginPage = withoutAuth(() => {
    return (
        <div className="grid h-full justify-items-center md:grid-cols-[1fr_1.5fr]">
            <section className="flex max-w-5xl flex-col items-center justify-center gap-10 px-10">
                <Image src={LogoText} alt="logo" />
                <SignupForm />
            </section>
            <AuthPageImage />
        </div>
    );
});
export default LoginPage;
