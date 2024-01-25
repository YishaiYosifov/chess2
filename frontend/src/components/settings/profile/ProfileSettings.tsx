"use client";

import { FormikHelpers } from "formik";

import styles from "./ProfileSettings.module.scss";
import { settingsApi } from "@/lib/apis";
import { ResponseError } from "@/client";
import constants from "@/lib/constants";

import UsernameForm, { UsernameSchema } from "./UsernameForm";
import ProfileForm, { ProfileSchema } from "./ProfileForm";
import EmailForm, { EmailSchema } from "./EmailForm";

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
                    helpers.setErrors({ email: "Email taken" });
                    break;
                default:
                    helpers.setStatus(constants.GENERIC_ERROR);
                    throw err;
            }
        }
    }

    async function updateProfile(
        values: ProfileSchema,
        helpers: FormikHelpers<ProfileSchema>
    ) {}

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
