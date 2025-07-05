import { headers } from "next/headers";

import GuestRedirect from "@/features/auth/components/GuestRedirect";
import constants from "@/lib/constants";

const page = async () => {
    const headerStore = await headers();
    let redirectTo =
        headerStore.get(constants.HEADERS.REDIRECT_AFTER_AUTH) ?? "/";
    if (redirectTo === constants.PATHS.GUEST) redirectTo = "/";

    return <GuestRedirect redirectTo={redirectTo} />;
};
export default page;
