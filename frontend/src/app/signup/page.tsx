import withoutAuth from "@/hocs/withoutAuth";
import AuthPage from "@/components/auth/AuthPage";

export const metadata = { title: "Signup - Chess 2" };

const SignupPage = withoutAuth(() => <AuthPage signup />);
export default SignupPage;
