"use client";

import { getAllCountries } from "countries-and-timezones";
import { useId, useMemo } from "react";

import FormikSubmitButton from "@/components/helpers/FormikSubmitButton";
import FormikField from "@/components/helpers/FormikField";
import { useAuthedProfile } from "@/hooks/useAuthed";
import Card from "@/components/helpers/Card";
import { Form, Formik } from "formik";

interface ProfileFormValues {
    about: string;
    countryCode?: string;
}

const ProfileSettings = () => {
    const { about, countryCode } = useAuthedProfile();
    const countries = useMemo(getAllCountries, []);

    const aboutMeId = useId();
    const countryId = useId();

    async function onSubmit(values: ProfileFormValues): Promise<void> {}

    return (
        <Card>
            <Formik initialValues={{ about, countryCode }} onSubmit={onSubmit}>
                <Form
                    aria-label="profile settings form"
                    className="flex w-full flex-col gap-8"
                >
                    <div className="flex w-full flex-col gap-5 text-lg">
                        <div>
                            <label htmlFor={aboutMeId}>About Me</label>
                            <FormikField
                                asInput="textarea"
                                name="about"
                                id={aboutMeId}
                                className="w-full rounded-md p-2 text-black"
                                maxLength={300}
                            />
                        </div>

                        <div>
                            <label htmlFor={countryId}>Country</label>

                            <FormikField
                                asInput="select"
                                name="countryCode"
                                className="w-full rounded-md text-black"
                                id={countryId}
                            >
                                <option>International</option>
                                {Object.values(countries).map((country) => (
                                    <option key={country.id} value={country.id}>
                                        {country.name}
                                    </option>
                                ))}
                            </FormikField>
                        </div>
                    </div>

                    <FormikSubmitButton>Save</FormikSubmitButton>
                </Form>
            </Formik>
        </Card>
    );
};
export default ProfileSettings;
