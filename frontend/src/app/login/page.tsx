import withoutAuth from "@/hocs/withoutAuth";

import LoginForm from "@/components/authPage/LoginForm";
import AuthPage from "@/components/authPage/AuthPage";

export const metadata = { title: "Login - Chess 2" };

const LoginPage = withoutAuth(() => {
    return <AuthPage form={<LoginForm />} />;
});
export default LoginPage;
