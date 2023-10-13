import { BsPlusSquare } from "react-icons/bs";

import SignupForm from "@/components/pages/auth/SignupForm";
import withoutAuth from "@/components/hocs/withoutAuth";
import AuthPage from "@/components/pages/auth/AuthPage";

export const metadata = {
    title: "Chess 2 - Signup",
};

const SignupPage = withoutAuth(() => <AuthPage signup />);
export default SignupPage;
