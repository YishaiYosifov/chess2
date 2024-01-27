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
        try {
            const profile = await settingsApi.updateProfile({
                editableProfile: values,
            });
            console.log(profile);
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
