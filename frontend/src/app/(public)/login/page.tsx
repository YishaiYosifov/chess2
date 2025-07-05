import AuthPage from "@/features/auth/components/AuthPage";
import withoutAuth from "@/features/auth/hocs/withoutAuth";

export const metadata = { title: "Login - Chess 2" };

const LoginPage = withoutAuth(() => {
    return <AuthPage />;
});
export default LoginPage;
