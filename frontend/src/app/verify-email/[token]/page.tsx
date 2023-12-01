import { apiRequest } from "@/lib/utils/fetchUtils";
import { redirect } from "next/navigation";

const VerifyEmailPage = async ({
    params: { token },
}: {
    params: { token: string };
}) => {
    const response = await apiRequest(`/auth/verify-email`, {
        json: { token },
    });

    switch (response.status) {
        case 200:
            redirect("/");
        default:
            redirect("/madge");
    }
};
export default VerifyEmailPage;
