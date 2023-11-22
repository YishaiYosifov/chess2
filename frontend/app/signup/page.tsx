import { BsPlusSquare } from "react-icons/bs";

import SignupFormValues from "@/components/auth/SignupForm";
import withoutAuth from "@/components/hocs/withoutAuth";
import AuthPage from "@/components/auth/AuthPage";

export const metadata = {
    title: "Chess 2 - Signup",
};

const SignupPage = withoutAuth(() => <AuthPage signup />);
export default SignupPage;
