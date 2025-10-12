import { redirect } from "next/navigation";

import WithAuthedUser from "@/features/auth/hocs/WithAuthedUser";

export default async function RedirectUserPage() {
    return (
        <WithAuthedUser>
            {({ user }) => {
                redirect(`/profile/${user.userName}`);
            }}
        </WithAuthedUser>
    );
}
