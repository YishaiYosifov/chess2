"use client";

import { Button, Form } from "react-bootstrap";
import { FormikHelpers, Formik } from "formik";

import {
    useAuthedContext,
    useAuthedProfile,
} from "@/components/contexts/AuthContext";
import styles from "./ProfileSettings.module.scss";
import { revalidateUser } from "@/app/actions";
import countries from "@/data/countries.json";
import { EditableProfile } from "@/client";
import { settingsApi } from "@/lib/apis";
import constants from "@/lib/constants";

import {
    FormInput,
    FormSelect,
    FormikField,
} from "@/components/form/FormElements";
import FormField from "@/components/form/FormField";

const ProfileSettings = () => {
    const { username, about, countryAlpha3, location, firstName, lastName } =
        useAuthedProfile();
    const { setAuthedProfile } = useAuthedContext();

    async function updateProfile(
        values: EditableProfile,
        helpers: FormikHelpers<EditableProfile>
    ) {
        try {
            const profile = await settingsApi.updateProfile({
                editableProfile: values,
            });
            setAuthedProfile(profile);
            revalidateUser(username);
        } catch (err) {
            helpers.setStatus(constants.GENERIC_ERROR);
            throw err;
        }
        helpers.resetForm();
    }

    return (
        <Formik
            onSubmit={updateProfile}
            initialValues={{
                about,
                countryAlpha3,
                location,
                firstName,
                lastName,
            }}
            enableReinitialize
        >
            {({ handleSubmit, dirty, isValid, status, isSubmitting }) => (
                <Form
                    className={styles["profile-settings"]}
                    aria-label="profile form"
                    noValidate
                    onSubmit={handleSubmit}
                >
                    <section className={styles["main-section"]}>
                        <FormField label="First Name" hasValidation>
                            <FormikField
                                data-testid="profileSettingsCountry"
                                asInput={FormInput}
                                name="firstName"
                                maxlength={50}
                            />
                        </FormField>

                        <FormField label="Last Name" hasValidation>
                            <FormikField
                                data-testid="profileSettingsCountry"
                                asInput={FormInput}
                                name="lastName"
                                maxlength={50}
                            />
                        </FormField>

                        <FormField label="Country" hasValidation>
                            <FormikField
                                data-testid="profileSettingsCountry"
                                asInput={FormSelect}
                                name="countryAlpha3"
                            >
                                {Object.entries(countries).map(
                                    ([alpha3, country]) => (
                                        <option key={alpha3} value={alpha3}>
                                            {country.name}
                                        </option>
                                    )
                                )}
                            </FormikField>
                        </FormField>

                        <FormField label="Location" hasValidation>
                            <FormikField
                                data-testid="profileSettingsCountry"
                                asInput={FormInput}
                                name="location"
                                maxlength={40}
                            />
                        </FormField>
                    </section>

                    <FormField label="About" hasValidation>
                        <FormikField
                            data-testid="profileSettingsAbout"
                            asInput={FormInput}
                            as="textarea"
                            name="about"
                            rows="5"
                            maxLength={300}
                            id="about"
                        />
                    </FormField>

                    <span className="text-invalid">{status}</span>

                    <Button
                        variant="dark"
                        type="submit"
                        disabled={!dirty || !isValid || isSubmitting}
                        data-testid="submitForm"
                    >
                        Save
                    </Button>
                </Form>
            )}
        </Formik>
    );
};
export default ProfileSettings;
