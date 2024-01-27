"use client";

import { FormikHelpers } from "formik";

import { useAuthedContext } from "@/components/contexts/AuthContext";
import styles from "./ProfileSettings.module.scss";
import { EditableProfile } from "@/client";
import { settingsApi } from "@/lib/apis";
import constants from "@/lib/constants";

import UsernameForm, { UsernameSchema } from "./UsernameForm";
import EmailForm, { EmailSchema } from "./EmailForm";
import ProfileForm from "./ProfileForm";

const ProfileSettings = () => {
    const { setAuthedProfile } = useAuthedContext();

    /**
     * Update a single field setting
     *
     * @param apiFunction - the api function binded to the api class
     * @param value - value to send
     * @param helpers - formik helpers
     */
    async function updateField<V>(
        apiFunction: (requestParams: { body: V }) => Promise<any>,
        value: V,
        helpers: FormikHelpers<any>
    ) {
        try {
            await apiFunction({
                body: value,
            });
            location.reload();
        } catch (err: any) {
            switch (err?.response?.status) {
                case 409:
                    helpers.setErrors((await err.response.json()).detail);
                    break;
                default:
                    helpers.setStatus(constants.GENERIC_ERROR);
                    throw err;
            }
        }
    }

    async function updateUsername(
        values: UsernameSchema,
        helpers: FormikHelpers<UsernameSchema>
    ) {
        updateField(
            settingsApi.changeUsername.bind(settingsApi),
            values.username,
            helpers
        );
    }

    async function updateEmail(
        values: EmailSchema,
        helpers: FormikHelpers<EmailSchema>
    ) {
        updateField(
            settingsApi.changeEmail.bind(settingsApi),
            values.email,
            helpers
        );
    }

    /**
     * Update the profile settings
     */
    async function updateProfile(
        values: EditableProfile,
        helpers: FormikHelpers<EditableProfile>
    ) {
        try {
            const profile = await settingsApi.updateProfile({
                editableProfile: values,
            });
            setAuthedProfile(profile);
        } catch (err) {
            helpers.setStatus(constants.GENERIC_ERROR);
            throw err;
        }
        helpers.resetForm();
    }

    return (
        <div className={styles["form-gap"]}>
            <UsernameForm onSubmit={updateUsername} />

            <hr />

            <EmailForm onSubmit={updateEmail} />

            <hr />

            <ProfileForm onSubmit={updateProfile} />
        </div>
    );
};
export default ProfileSettings;
