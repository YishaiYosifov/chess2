import AuthPage from "@/components/authPage/AuthPage";
import withoutAuth from "@/hocs/withoutAuth";

export const metadata = { title: "Signup - Chess 2" };

const SignupPage = withoutAuth(() => <AuthPage />);
export default SignupPage;
