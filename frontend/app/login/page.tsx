import { BsBoxArrowInLeft } from "react-icons/bs";

import withoutAuth from "@/components/hocs/withoutAuth";
import LoginForm from "@/components/pages/LoginForm";

export const metadata = {
    title: "Chess 2 - Login",
};

const LoginPage = withoutAuth(async () => {
    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-xl-6 col-lg-8 col-md-10 mt-xxl-5">
                    <h3 className="text-start text-lg-center">
                        <BsBoxArrowInLeft className="me-1 me-sm-2" />
                        Log into Your Account
                    </h3>

                    <div className="card mt-4">
                        <LoginForm />
                    </div>
                </div>
            </div>
        </div>
    );
});

export default LoginPage;
