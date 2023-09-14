import { BsBoxArrowInLeft } from "react-icons/bs";
import { redirect } from "next/navigation";

import LoginForm from "../components/pages/LoginForm";
import { getServerSession } from "../utils/server";

export const metadata = {
    title: "Chess 2 - Login",
};

const LoginPage = async () => {
    const session = getServerSession();
    if (session.isAuthed) redirect("/");

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
};
export default LoginPage;
