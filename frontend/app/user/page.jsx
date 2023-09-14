import { redirect } from "next/navigation";
import { useStore } from "../store";

const RedirectUserPage = async () => {
    const { isAuthed, profile } = useStore.getState();
    if (!isAuthed) redirect("/");

    redirect(`/user/${profile.username}`);
};
export default RedirectUserPage;
