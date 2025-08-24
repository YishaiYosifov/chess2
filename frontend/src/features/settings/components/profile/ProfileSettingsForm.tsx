"use client";

import dynamic from "next/dynamic";
import { Form, Formik } from "formik";

import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";
import FormikSubmitButton from "@/components/ui/FormikSubmitButton";
import FormikTextField from "@/components/ui/FormikField";
import Card from "@/components/ui/Card";

const CountrySelector = dynamic(
    () => import("@/features/settings/components/CountrySelector"),
    { ssr: false },
);

interface ProfileSettingsValues {
    userName: string;
    about: string;
    country: string;
}

const ProfileSettingsForm = () => {
    const user = useAuthedUser();
    if (!user) return null;

    async function handleSubmit(values: ProfileSettingsValues) {
        console.log(values);
    }

    return (
        <Formik
            initialValues={{
                userName: user.userName,
                about: user.about,
                country: user.countryCode,
            }}
            onSubmit={handleSubmit}
        >
            <Form>
                <Card className="gap-5">
                    <div>
                        <FormikTextField name="userName" label="Username" />
                        <span className="text-text/60 text-sm">
                            Can only be changed once every 2 weeks
                        </span>
                    </div>

                    <FormikTextField
                        label="About Me"
                        as="textarea"
                        className="min-h-60"
                        maxLength={500}
                        name="about"
                    />

                    <CountrySelector name="country" />

                    <FormikSubmitButton type="submit">Save</FormikSubmitButton>
                </Card>
            </Form>
        </Formik>
    );
};
export default ProfileSettingsForm;
