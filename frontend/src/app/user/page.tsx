import { redirect } from "next/navigation";

import { PrivateAuthedProfileOut } from "@/models";
import withAuth from "@/hocs/withAuth";

const RedirectUserPage = withAuth(
    async ({ profile }: { profile: PrivateAuthedProfileOut }) => {
        redirect(`/user/${profile.username}`);
    }
);
export default RedirectUserPage;
