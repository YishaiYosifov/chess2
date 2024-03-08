import withoutAuth from "@/hocs/withoutAuth";
import AuthPage from "@/components/auth/AuthPage";

export const metadata = { title: "Login - Chess 2" };

const LoginPage = withoutAuth(() => <AuthPage login />);
export default LoginPage;
