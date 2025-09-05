"use client";

import { Form, Formik, FormikHelpers } from "formik";

import { InteractionLevel, Preferences, setPreferences } from "@/lib/apiClient";
import FormikSubmitButton from "@/components/ui/FormikSubmitButton";
import FormField from "@/components/ui/FormField";
import Selector from "@/components/ui/Selector";
import Card from "@/components/ui/Card";

const PrivacyForm = ({
    initialPreferences,
}: {
    initialPreferences: Preferences;
}) => {
    async function onSubmit(
        values: Preferences,
        helpers: FormikHelpers<Preferences>,
    ) {
        const { error } = await setPreferences({ body: values });
        if (error) {
            console.error(error);
            helpers.setStatus("Failed to update preferences");
            return;
        }
        helpers.resetForm({ values });
    }

    return (
        <Formik initialValues={initialPreferences} onSubmit={onSubmit}>
            <Form>
                <Card className="gap-10">
                    <FormField
                        label="Allow Challenges"
                        name="challengePreference"
                    >
                        <Selector
                            data-testid="challengePreference"
                            options={[
                                {
                                    label: "Never",
                                    value: InteractionLevel.NO_ONE,
                                },
                                {
                                    label: "Only Stars",
                                    value: InteractionLevel.STARRED,
                                },
                                {
                                    label: "Always",
                                    value: InteractionLevel.EVERYONE,
                                },
                            ]}
                        />
                    </FormField>

                    <hr className="text-secondary/30" />

                    <FormField label="Show Chat by Default" name="showChat">
                        <Selector
                            data-testid="showChat"
                            options={[
                                {
                                    label: "Yes",
                                    value: true,
                                },
                                {
                                    label: "No",
                                    value: false,
                                },
                            ]}
                        />
                    </FormField>

                    <hr className="text-secondary/30" />

                    <FormikSubmitButton>Save</FormikSubmitButton>
                </Card>
            </Form>
        </Formik>
    );
};
export default PrivacyForm;
