import { BsPlusSquare } from "react-icons/bs";
import { redirect } from "next/navigation";

import SignupForm from "../components/pages/SignupForm";
import { getServerSession } from "../utils/server";

export const metadata = {
    title: "Chess 2 - Signup",
};

const SignupPage = async () => {
    const session = getServerSession();
    if (session.isAuthed) redirect("/");

    return (
        <div className="container mt-5">
            <div className="row justify-content-center">
                <div className="col-xl-6 col-lg-8 col-md-10 mt-xxl-5">
                    <h3 className="text-start text-lg-center">
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
};
export default SignupPage;
