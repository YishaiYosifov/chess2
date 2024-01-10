"use client";

import { Formik } from "formik";
import { Form } from "react-bootstrap";

import * as yup from "yup";

import { emailSchema, usernameSchema } from "@/lib/validation";
import { USERNAME_EDIT_EVERY } from "@/lib/constants";
import styles from "./ProfileForm.module.scss";
import { useStore } from "@/zustand/store";

import { FormikInput, FormikSelect } from "@/components/FormikElements";
import PasswordField from "../fields/PasswordField";
import FormField from "@/components/FormField";

const usernameSettingSchema = yup.object({ username: usernameSchema });
const emailSettingSchema = yup.object({ email: emailSchema });
const profileSettingsSchema = yup.object();

/**
 * Component for the settings form
 */
const ProfileSettingsForm = () => {
    const profile = useStore.use.localProfile();

    /**
     * Check if the given field has recently changed
     *
     * @param editEvery - how often should you be able to edit this field in seconds
     * @param lastChanged - the timestamp of the last change
     * @returns whether the field has changed recently
     */
    function didChangeRecently(
        editEvery: number,
        lastChanged?: Date | null
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

    async function updateUsername() {}
    async function updateEmail() {}

    return (
        <div className={styles["form-gap"]}>
            {usernameChangedRecently && (
                <span className={styles["setting-disabled-label"]}>
                    username changed too recently
                </span>
            )}

            <PasswordField
                field="username"
                fieldLabel="Username"
                defaultValue={profile.username}
                disabled={usernameChangedRecently}
                schema={usernameSettingSchema}
                onSubmit={updateUsername}
            />

            <hr />

            <PasswordField
                field="email"
                fieldLabel="Username"
                defaultValue={profile.email}
                schema={emailSettingSchema}
                onSubmit={updateEmail}
            />

            <hr />

            <Formik
                validationSchema={profileSettingsSchema}
                initialValues={{
                    about: "",
                    country: "",
                }}
            >
                {({ handleSubmit, isSubmitting, status }) => (
                    <Form
                        className={styles["form-gap"]}
                        aria-label="profile form"
                        noValidate
                        onSubmit={handleSubmit}
                    >
                        <FormField fieldLabel="About" hasValidation>
                            <FormikInput fieldName="about" as="textarea" />
                        </FormField>

                        <FormField fieldLabel="Country" hasValidation>
                            <FormikSelect fieldName="country"></FormikSelect>
                        </FormField>
                    </Form>
                )}
            </Formik>
        </div>
    );
};
export default ProfileSettingsForm;
