import { Formik, FormikHelpers } from "formik";
import { Button, Form } from "react-bootstrap";
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

const profileSettingsSchema = yup.object();

const ProfileForm = ({
    onSubmit,
}: {
    onSubmit: (values: Object, helpers: FormikHelpers<any>) => Promise<void>;
}) => {
    const { about } = useLoadedProfile();

    return (
        <Formik
            validationSchema={profileSettingsSchema}
            onSubmit={onSubmit}
            initialValues={{
                about: about,
                country: "INTR",
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
                        <FormikField asInput={FormSelect} name="country">
                            {countries.map((country) => (
                                <option
                                    key={country.alpha3}
                                    value={country.alpha3}
                                >
                                    {country.name}
                                </option>
                            ))}
                        </FormikField>
                    </FormField>

                    <Button variant="dark">Save</Button>
                </Form>
            )}
        </Formik>
    );
};
export default ProfileForm;
