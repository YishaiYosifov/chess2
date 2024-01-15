"use client";

import { FormikHelpers } from "formik";

import styles from "./ProfileSettings.module.scss";
import { settingsApi } from "@/lib/apis";

import UsernameForm, { UsernameSchema } from "./UsernameForm";
import EmailForm, { EmailSchema } from "./EmailForm";
import ProfileForm from "./ProfileForm";

const ProfileSettings = () => {
    async function updateUsername(
        values: UsernameSchema,
        helpers: FormikHelpers<UsernameSchema>
    ) {
        try {
            await settingsApi.changeUsername({
                body: values.username,
            });
            location.reload();
        } catch {
            helpers.setStatus("tee hee");
        }
    }

    async function updateEmail(
        values: EmailSchema,
        helpers: FormikHelpers<EmailSchema>
    ) {
        try {
            await settingsApi.changeEmail({
                body: values.email,
            });
            location.reload();
        } catch {
            helpers.setStatus("tee hee");
        }
    }

    async function updateProfile() {}

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
