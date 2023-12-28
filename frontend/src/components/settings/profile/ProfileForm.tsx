"use client";

import { Formik, FormikHelpers } from "formik";
import { Form } from "react-bootstrap";

import * as yup from "yup";

import { USERNAME_EDIT_EVERY } from "@/lib/constants";
import { apiRequest } from "@/lib/utils/fetchUtils";
import styles from "./ProfileForm.module.scss";
import { useStore } from "@/zustand/store";

import { FormikField } from "@/components/FormField";
import PasswordField from "../fields/PasswordField";

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

/**
 * Component for the settings form
 */
const SettingsForm = () => {
    const profile = useStore.use.localProfile();

    const usernameSchema = yup
        .object()
        .shape({ username: yup.string().username() });
    const emailSchema = yup.object().shape({ email: yup.string().email() });

    /**
     * Check if the given field has recently changed
     *
     * @param editEvery - how often should you be able to edit this field in seconds
     * @param lastChanged - the timestamp of the last change
     * @returns whether the field has changed recently
     */
    function didChangeRecently(
        editEvery: number,
        lastChanged?: string
    ): boolean {
        if (!lastChanged) return false;

        const lastChangedDate = new Date(lastChanged).valueOf();
        const now = new Date().valueOf();
        const timeSinceChange = (now - lastChangedDate) / 1000;

        return timeSinceChange < editEvery;
    }

    const usernameChangedRecently = didChangeRecently(
        USERNAME_EDIT_EVERY,
        profile.usernameLastChanged
    );

    return (
        <>
            {usernameChangedRecently && (
                <span className={styles["setting-disabled-label"]}>
                    username changed too recently
                </span>
            )}

            <PasswordField
                field="username"
                defaultValue={profile.username}
                disabled={usernameChangedRecently}
                schema={usernameSchema}
                onSubmit={updateSettings}
                fieldLabel
            />

            <hr />

            <PasswordField
                field="email"
                defaultValue={profile.email}
                schema={emailSchema}
                onSubmit={updateSettings}
                fieldLabel
            />
        </>
    );
};
export default SettingsForm;
