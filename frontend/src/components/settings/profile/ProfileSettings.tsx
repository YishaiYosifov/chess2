"use client";

import { FormikHelpers } from "formik";

import styles from "./ProfileSettings.module.scss";
import { EditableProfile } from "@/client";
import { settingsApi } from "@/lib/apis";
import constants from "@/lib/constants";

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
        } catch (err: any) {
            switch (err?.response?.status) {
                case 409:
                    helpers.setErrors({ username: "Username taken" });
                    break;
                default:
                    helpers.setStatus(constants.GENERIC_ERROR);
                    throw err;
            }
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

    async function updateProfile(
        values: EditableProfile,
        helpers: FormikHelpers<EditableProfile>
    ) {
        console.log("test");
        try {
            await settingsApi.updateProfile({ editableProfile: values });
            location.reload();
        } catch (err: any) {
            switch (err?.response?.status) {
                case 422:
                    helpers.setErrors((await err.response.json()).detail);
                    break;
                default:
                    helpers.setStatus(constants.GENERIC_ERROR);
                    throw err;
            }
        }
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
