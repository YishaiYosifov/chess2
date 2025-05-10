import AuthPage from "@/components/authPage/AuthPage";
import withoutAuth from "@/hocs/withoutAuth";

export const metadata = { title: "Login - Chess 2" };

const LoginPage = withoutAuth(() => {
    return <AuthPage />;
});
export default LoginPage;
