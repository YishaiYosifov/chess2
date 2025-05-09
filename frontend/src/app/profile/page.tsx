import { redirect } from "next/navigation";

import withAuth from "@/hocs/withAuth";
import { auth } from "@/lib/auth";

// redirect the user to their profile page
const RedirectUserPage = withAuth(async () => {
    const session = await auth();
    //redirect(`/profile/${session?.user.userName}`);
});
export default RedirectUserPage;
