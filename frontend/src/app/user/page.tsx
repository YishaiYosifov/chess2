import { redirect } from "next/navigation";

import { PrivateAuthedProfileOut } from "@/apiClient";
import withAuth from "@/hocs/withAuth";

const RedirectUserPage = withAuth(
    async ({ profile }: { profile: PrivateAuthedProfileOut }) => {
        redirect(`/user/${profile.username}`);
    }
);
export default RedirectUserPage;
