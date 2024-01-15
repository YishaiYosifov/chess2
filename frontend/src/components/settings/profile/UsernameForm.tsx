import { Formik, FormikHelpers } from "formik";

import * as yup from "yup";

import { useLoadedProfile } from "@/zustand/store";
import { usernameSchema } from "@/lib/validation";
import styles from "./UsernameForm.module.scss";
import constants from "@/lib/constants";

import CollapsibleForm from "@/components/form/CollapsibleForm";
import { FormInput, FormikField } from "@/components/form/FormElements";
import FormField from "@/components/form/FormField";

export interface UsernameSchema {
    username: string;
}

const usernameSettingSchema = yup.object({ username: usernameSchema });

const UsernameForm = ({
    onSubmit,
}: {
    onSubmit: (
        values: UsernameSchema,
        helpers: FormikHelpers<UsernameSchema>
    ) => void;
}) => {
    const { usernameLastChanged, username } = useLoadedProfile();

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
        constants.USERNAME_EDIT_EVERY,
        usernameLastChanged
    );

    return (
        <>
            {usernameChangedRecently && (
                <span className={styles["setting-disabled-label"]}>
                    username changed too recently
                </span>
            )}

            <Formik
                validationSchema={usernameSettingSchema}
                onSubmit={onSubmit}
                initialValues={{ username }}
            >
                {({ handleSubmit }) => (
                    <CollapsibleForm
                        label="Username"
                        disabled={usernameChangedRecently}
                        defaultValue={username}
                        onSubmit={handleSubmit}
                    >
                        <FormField label="Username" hasValidation>
                            <FormikField asInput={FormInput} name="username" />
                        </FormField>
                    </CollapsibleForm>
                )}
            </Formik>
        </>
    );
};
export default UsernameForm;
