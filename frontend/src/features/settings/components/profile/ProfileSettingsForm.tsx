"use client";

import dynamic from "next/dynamic";
import { Form, Formik, FormikHelpers } from "formik";

import {
    useAuthedUser,
    useSessionStore,
} from "@/features/auth/hooks/useSessionUser";

import FormikSubmitButton from "@/components/ui/FormikSubmitButton";
import { editProfileSettings, PrivateUser } from "@/lib/apiClient";
import FormikTextField from "@/components/ui/FormikField";
import Card from "@/components/ui/Card";

const CountrySelector = dynamic(() => import("./CountrySelector"), {
    ssr: false,
});

interface ProfileSettingsValues {
    about: string;
    countryCode: string;
}

const ProfileSettingsForm = () => {
    const user = useAuthedUser();
    const setUser = useSessionStore((x) => x.setUser);

    if (!user) return null;

    async function handleSubmit(
        values: ProfileSettingsValues,
        helpers: FormikHelpers<ProfileSettingsValues>,
    ) {
        if (!user) return;

        const { error } = await editProfileSettings({ body: values });
        if (error) {
            helpers.setStatus("Failed to save profile settings");
            console.error(error);
            return;
        }

        const newUser: PrivateUser = {
            ...user,
            ...values,
        };
        setUser(newUser);
        helpers.resetForm({ values });
    }

    return (
        <Formik
            initialValues={{
                about: user.about,
                countryCode: user.countryCode,
            }}
            onSubmit={handleSubmit}
        >
            <Form>
                <Card>
                    <FormikTextField
                        data-testid="aboutMeSetting"
                        label="About Me"
                        as="textarea"
                        className="min-h-60"
                        maxLength={500}
                        name="about"
                    />

                    <CountrySelector name="countryCode" />

                    <FormikSubmitButton>Save</FormikSubmitButton>
                </Card>
            </Form>
        </Formik>
    );
};
export default ProfileSettingsForm;
