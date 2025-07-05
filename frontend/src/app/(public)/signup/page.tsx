import AuthPage from "@/features/auth/components/AuthPage";
import withoutAuth from "@/features/auth/hocs/withoutAuth";

export const metadata = { title: "Signup - Chess 2" };

const SignupPage = withoutAuth(() => <AuthPage />);
export default SignupPage;
