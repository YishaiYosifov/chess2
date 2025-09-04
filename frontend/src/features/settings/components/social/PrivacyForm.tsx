import { Form, Formik, FormikHelpers } from "formik";

import FormikSubmitButton from "@/components/ui/FormikSubmitButton";
import Selector from "@/components/ui/Selector";
import Card from "@/components/ui/Card";
import FormField from "@/components/ui/FormField";
import {
    getPreferences,
    InteractionLevel,
    Preferences,
    setPreferences,
} from "@/lib/apiClient";
import { useEffect, useState } from "react";

const PrivacyForm = () => {
    const [initialPreferences, setInitialPreferences] =
        useState<Preferences | null>(null);

    useEffect(() => {
        async function fetchPreferences() {
            const { error, data } = await getPreferences();
            if (error || data === undefined) console.error(error);
            else setInitialPreferences(data);
        }
        fetchPreferences();
    }, []);
    if (!initialPreferences) return null;

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

                    <FormField label="Show Chat" name="chatPreference">
                        <Selector
                            data-testid="chatPreference"
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

                    <FormikSubmitButton>Save</FormikSubmitButton>
                </Card>
            </Form>
        </Formik>
    );
};
export default PrivacyForm;
