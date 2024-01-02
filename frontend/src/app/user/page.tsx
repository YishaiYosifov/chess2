import { redirect } from "next/navigation";

import withAuth from "@/components/hocs/withAuth";
import { PrivateUserOut } from "@/client";

const RedirectUserPage = withAuth(
    async ({ profile }: { profile: PrivateUserOut }) => {
        redirect(`/user/${profile.username}`);
    }
);
export default RedirectUserPage;
