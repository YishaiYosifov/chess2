"use client";

import * as yup from "yup";

import Card from "@/components/ui/Card";
import FormikSubmitButton from "@/components/ui/FormikSubmitButton";
import {
    useAuthedUser,
    useSessionStore,
} from "@/features/auth/hooks/useSessionUser";
import { editUsername, PrivateUser } from "@/lib/apiClient";
import constants from "@/lib/constants";
import { UsernameSchema } from "@/lib/validation";
import { Form, Formik, FormikHelpers } from "formik";
import InputField from "@/components/ui/InputField";
import FormField from "@/components/ui/FormField";

interface UsernameFormValues {
    userName: string;
}

const UsernameFormSchema = yup.object().shape({ userName: UsernameSchema });

const UsernameSettingsForm = () => {
    const user = useAuthedUser();
    const setUser = useSessionStore((x) => x.setUser);

    if (!user) return null;

    function cooldownUntil(): Date | null {
        if (!user || !user.usernameLastChanged) return null;

        const nextUsernameChange = new Date(
            new Date(user.usernameLastChanged).valueOf() +
                constants.USERNAME_EDIT_EVERY_MS,
        );
        if (nextUsernameChange <= new Date()) return null;

        return nextUsernameChange;
    }
    const nextUsernameChangeDate = cooldownUntil();

    async function handleSubmit(
        values: UsernameFormValues,
        helpers: FormikHelpers<UsernameFormValues>,
    ): Promise<void> {
        if (!user) return;

        const { error } = await editUsername({
            body: { username: values.userName },
        });
        if (error) {
            helpers.setStatus("Failed to edit username");
            console.error(error);
            return;
        }

        const newUser: PrivateUser = {
            ...user,
            userName: values.userName,
            usernameLastChanged: new Date().toISOString(),
        };
        setUser(newUser);
        helpers.resetForm({ values });
    }

    return (
        <Formik
            initialValues={{ userName: user.userName }}
            validationSchema={UsernameFormSchema}
            onSubmit={handleSubmit}
        >
            <Form>
                <Card className="gap-5">
                    <div>
                        <FormField label="Username" name="userName">
                            <InputField
                                data-testid="usernameSettingField"
                                disabled={nextUsernameChangeDate !== null}
                                maxLength={30}
                            />
                        </FormField>
                        <span className="text-text/60 text-sm">
                            {nextUsernameChangeDate
                                ? `Can changed again on ${nextUsernameChangeDate.toLocaleDateString("en-US")}`
                                : "Can only be changed once every 2 weeks"}
                        </span>
                    </div>

                    <FormikSubmitButton>Save</FormikSubmitButton>
                </Card>
            </Form>
        </Formik>
    );
};
export default UsernameSettingsForm;
