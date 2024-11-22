import { redirect } from "next/navigation";

import withAuth from "@/hocs/withAuth";

const RedirectUserPage = withAuth(async ({ profile }) => {
    redirect(`/profile/${profile.username}`);
});
export default RedirectUserPage;
