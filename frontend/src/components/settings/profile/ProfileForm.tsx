import { Button, Form } from "react-bootstrap";
import { Formik } from "formik";
import * as yup from "yup";

import styles from "./ProfileSettings.module.scss";
import countries from "@/data/countries.json";

import {
    FormInput,
    FormSelect,
    FormikField,
} from "@/components/form/FormElements";
import FormField from "@/components/form/FormField";
import { useLoadedProfile } from "@/zustand/store";
import { FormikOnSubmit } from "@/lib/types";
import { EditableProfile } from "@/client";

const profileSettingsSchema = yup.object();

const ProfileForm = ({
    onSubmit,
}: {
    onSubmit: FormikOnSubmit<EditableProfile>;
}) => {
    const { about, countryAlpha3 } = useLoadedProfile();

    return (
        <Formik
            validationSchema={profileSettingsSchema}
            onSubmit={onSubmit}
            initialValues={{
                about,
                countryAlpha3,
            }}
        >
            {({ handleSubmit }) => (
                <Form
                    className={styles["form-gap"]}
                    aria-label="profile form"
                    noValidate
                    onSubmit={handleSubmit}
                >
                    <FormField label="About" hasValidation>
                        <FormikField
                            asInput={FormInput}
                            as="textarea"
                            name="about"
                            maxLength={300}
                        />
                    </FormField>

                    <FormField label="Country" hasValidation>
                        <FormikField asInput={FormSelect} name="countryAlpha3">
                            {Object.entries(countries).map(
                                ([alpha3, country]) => (
                                    <option key={alpha3} value={alpha3}>
                                        {country.name}
                                    </option>
                                )
                            )}
                        </FormikField>
                    </FormField>

                    <Button variant="dark" type="submit">
                        Save
                    </Button>
                </Form>
            )}
        </Formik>
    );
};
export default ProfileForm;
