import { redirect } from "next/navigation";

import { PrivateAuthedProfileOut } from "@/client";
import withAuth from "@/components/hocs/withAuth";

const RedirectUserPage = withAuth(
    async ({ profile }: { profile: PrivateAuthedProfileOut }) => {
        redirect(`/user/${profile.username}`);
    }
);
export default RedirectUserPage;
