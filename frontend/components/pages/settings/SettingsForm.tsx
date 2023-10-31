"use client";

import { FormikHelpers } from "formik";
import * as yup from "yup";

import { EMAIL_EDIT_EVERY, USERNAME_EDIT_EVERY } from "@/lib/constants";
import { apiRequest } from "@/lib/utils/common";
import { useStore } from "@/zustand/store";

import PasswordField from "./specialFields/PasswordField";

export async function updateSettings(
    values: object,
    helpers: FormikHelpers<object>
) {
    const settingsResponse = await apiRequest("/profile/update-settings", {
        method: "PUT",
        json: values,
    });
    helpers.setSubmitting(false);
    const msg = (await settingsResponse.json()).msg;

    switch (settingsResponse.status) {
        case 200:
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

const SettingsForm = () => {
    const profile = useStore.use.localProfile();

    const usernameSchema = yup
        .object()
        .shape({ username: yup.string().username() });
    const emailSchema = yup.object().shape({ email: yup.string().email() });

    function canEdit(lastChanged: string, editEvery: number): Boolean {
        const lastChangedDate = new Date(lastChanged).valueOf();
        const now = new Date().valueOf();

        const timeSinceChange = (now - lastChangedDate) / 1000;
        return timeSinceChange > editEvery;
    }

    return (
        <div className="row g-3">
            {/* username */}
            <PasswordField
                field="username"
                defaultValue={profile.username}
                editingError={
                    !canEdit(
                        profile.usernameLastChanged || "",
                        USERNAME_EDIT_EVERY
                    ) && "Username changed too recently"
                }
                schema={usernameSchema}
                onSubmit={updateSettings}
            />

            {/* email */}
            <PasswordField
                field="email"
                defaultValue={profile.email}
                schema={emailSchema}
                editingError={
                    !canEdit(
                        profile.emailLastChanged || "",
                        EMAIL_EDIT_EVERY
                    ) && "Email changed too recently"
                }
                onSubmit={updateSettings}
            />
        </div>
    );
};
export default SettingsForm;
