import { redirect } from "next/navigation";
import { useStore } from "../store";

const RedirectUserPage = async () => {
    redirect(`/user/luka`);
    const { isAuthed } = getServerSession();
    if (!isAuthed) redirect("/");

    const { profile } = useStore.getState();
    redirect(`/user/${profile.username}`);
};
export default RedirectUserPage;
