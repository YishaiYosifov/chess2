import { redirect } from "next/navigation";

import withAuth from "@/hocs/withAuth";

// redirect the user to their profile page
const RedirectUserPage = withAuth(async () => {
    //redirect(`/profile/${session?.user.userName}`);
});
export default RedirectUserPage;
