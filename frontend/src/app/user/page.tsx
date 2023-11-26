import { redirect } from "next/navigation";

import type { LocalProfile } from "@/lib/types";
import withAuth from "@/components/hocs/withAuth";

const RedirectUserPage = withAuth(
    async ({ profile }: { profile: LocalProfile }) => {
        redirect(`/user/${profile.username}`);
    }
);
export default RedirectUserPage;
