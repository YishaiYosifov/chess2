import { redirect } from "next/navigation";

import getConfiguredServerSession from "@/lib/auth/getConfiguredServerSession";
import withAuth from "@/hocs/withAuth";

// redirect the user to their profile page
const RedirectUserPage = withAuth(async () => {
    const session = await getConfiguredServerSession();
    //redirect(`/profile/${session?.user.userName}`);
});
export default RedirectUserPage;
