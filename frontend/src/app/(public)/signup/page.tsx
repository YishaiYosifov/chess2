import withoutAuth from "@/hocs/withoutAuth";

import SignupForm from "@/components/authPage/SignupForm";
import AuthPage from "@/components/authPage/AuthPage";

export const metadata = { title: "Signup - Chess 2" };

const SignupPage = withoutAuth(() => <AuthPage form={<SignupForm />} />);
export default SignupPage;
