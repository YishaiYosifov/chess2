import withoutAuth from "@/components/hocs/withoutAuth";
import AuthPage from "@/components/auth/AuthPage";

export const metadata = {
    title: "Chess 2 - Login",
};

const LoginPage = withoutAuth(() => <AuthPage login />);
export default LoginPage;
