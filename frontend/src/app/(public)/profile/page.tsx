import { redirect } from "next/navigation";

import withAuthedUser from "@/features/auth/hocs/withAuthedUser";

// redirect the user to their profile page
const RedirectUserPage = withAuthedUser(async ({ user }) =>
    redirect(`/profile/${user.userName}`),
);
export default RedirectUserPage;
