import { Formik } from "formik";
import * as yup from "yup";

import { useAuthedProfile } from "@/components/contexts/AuthContext";
import { emailSchema } from "@/lib/validation";
import { FormikOnSubmit } from "@/lib/types";

import { FormInput, FormikField } from "@/components/form/FormElements";
import FreshAuthForm from "@/components/form/FreshAuthForm";
import FormField from "@/components/form/FormField";

export interface EmailSchema {
    email: string;
}

const emailSettingSchema = yup.object({ email: emailSchema });

const EmailForm = ({ onSubmit }: { onSubmit: FormikOnSubmit<EmailSchema> }) => {
    const { email } = useAuthedProfile();

    return (
        <Formik
            validationSchema={emailSettingSchema}
            onSubmit={onSubmit}
            initialValues={{ email }}
        >
            {({ handleSubmit }) => (
                <FreshAuthForm
                    label="Email"
                    defaultValue={email}
                    onSubmit={handleSubmit}
                >
                    <FormField label="Email" hasValidation>
                        <FormikField asInput={FormInput} name="email" />
                    </FormField>
                </FreshAuthForm>
            )}
        </Formik>
    );
};
export default EmailForm;
