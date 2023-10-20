import { redirect } from "next/navigation";

import { LocalProfile } from "@/zustand/slices/authSlice";
import withAuth from "@/components/hocs/withAuth";

const RedirectUserPage = withAuth(
    async ({ profile }: { profile: LocalProfile }) => {
        redirect(`/user/${profile.username}`);
    }
);
export default RedirectUserPage;
