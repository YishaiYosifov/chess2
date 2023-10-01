import { BsPlusSquare } from "react-icons/bs";

import SignupForm from "@/components/pages/SignupForm";
import withoutAuth from "@/components/hocs/withoutAuth";

export const metadata = {
    title: "Chess 2 - Signup",
};

const SignupPage = withoutAuth(async () => {
    return (
        <div className="container mt-4">
            <div className="row justify-content-center">
                <div className="col-xl-6 col-lg-8 col-md-10 mt-xxl-5">
                    <h3 className="text-start text-md-center">
                        <BsPlusSquare className="me-1 me-sm-2" />
                        Create an Account
                    </h3>

                    <div className="card mt-4">
                        <SignupForm />
                    </div>
                </div>
            </div>
        </div>
    );
});
export default SignupPage;
