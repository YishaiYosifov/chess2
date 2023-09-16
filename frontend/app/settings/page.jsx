import { BsGearFill } from "react-icons/bs";

import { updateProfile, useStore } from "../store";

import SettingsForm from "../components/pages/settings/SettingsForm";
import InitializeStore from "../components/InitializeStore";
import ChangeProfilePicture from "../components/pages/settings/ChangeProfilePicture";
import { apiRequest } from "../utils/common";

const COL_CLASSES = "col-11 col-lg-9 col-xl-8 col-xxl-7 mx-auto";

export async function updateSettings(form, helpers) {
    const settingsResponse = await apiRequest("/profile/update-settings", {
        method: "PUT",
        json: form,
    });
    helpers.setSubmitting(false);
    const msg = (await settingsResponse.json()).msg;

    switch (settingsResponse.status) {
        case 200:
            delete form.password_confirm;
            updateProfile(form);
            window.location.reload();
            break;
        case 400:
            helpers.setErrors(msg);
            break;
        default:
            helpers.setStatus("Something went wrong.");
            console.error(msg);
            break;
    }
}

const SettingsPage = async () => {
    const { isAuthed, profile } = useStore.getState();
    //if (!isAuthed) redirect("/login");

    const countries = await (
        await fetch(`${process.env.NEXT_PUBLIC_API_URL}/static/countries.json`)
    ).json();

    return (
        <div className="container-fluid" style={{ maxWidth: "1426px" }}>
            <InitializeStore
                values={{ settings: useStore.getState().settings }}
            />

            <header className="row">
                <div className={COL_CLASSES}>
                    <h1 className="text-light">
                        <BsGearFill /> Settings
                    </h1>
                </div>
            </header>

            <section className="mt-4">
                <div className="row">
                    <div className={COL_CLASSES}>
                        <h4>Profile Picture</h4>
                    </div>
                </div>
                <div className="row">
                    <div className={`card ${COL_CLASSES}`}>
                        <ChangeProfilePicture />
                    </div>
                </div>
            </section>

            <section className="mt-4">
                <div className="row">
                    <h4 className={COL_CLASSES}>Profile Settings</h4>
                </div>
                <div className="row">
                    <div className={`card ${COL_CLASSES}`}>
                        <SettingsForm countries={countries} />
                    </div>
                </div>
            </section>
        </div>
    );
};
export default SettingsPage;
