import { headers } from "next/headers";

import Guest from "@/components/auth/Guest";
import constants from "@/lib/constants";

const page = async () => {
    const headerStore = await headers();
    let redirectTo =
        headerStore.get(constants.HEADERS.REDIRECT_AFTER_AUTH) ?? "/";
    if (redirectTo === constants.PATHS.GUEST) redirectTo = "/";

    return <Guest redirectTo={redirectTo} />;
};
export default page;
