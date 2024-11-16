import AuthPageImage from "@/components/auth/AuthPageImage";
import LoginForm from "@/components/auth/LoginForm";
import withoutAuth from "@/hocs/withoutAuth";

export const metadata = { title: "Login - Chess 2" };

const LoginPage = withoutAuth(() => {
    return (
        <div className="grid md:grid-cols-[1fr_1.5fr] h-full">
            <LoginForm />
            <AuthPageImage />
        </div>
    );
});
export default LoginPage;
