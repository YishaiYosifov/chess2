import { Form, Formik } from "formik";

import FormikSubmitButton from "@/components/ui/FormikSubmitButton";
import TextField from "@/components/ui/TextField";
import Selector from "@/components/ui/Selector";
import Card from "@/components/ui/Card";

const PrivacyForm = () => {
    return (
        <Formik>
            <Form>
                <Card className="gap-10">
                    <TextField
                        label="Allow Friend Requests"
                        as={Selector}
                        options={{ Yes: true, No: false }}
                        defaultValue={true}
                    />

                    <hr className="text-secondary" />

                    <TextField
                        label="Allow Challenges"
                        as={Selector}
                        options={{
                            Never: true,
                            "Only Friends": false,
                            Always: true,
                        }}
                        defaultValue={true}
                    />

                    <hr className="text-secondary" />

                    <TextField
                        label="Show Chats"
                        as={Selector}
                        options={{
                            Never: true,
                            "Only Friends": false,
                            Always: true,
                        }}
                        defaultValue={true}
                    />

                    <hr className="text-secondary/30" />

                    <FormikSubmitButton>Save</FormikSubmitButton>
                </Card>
            </Form>
        </Formik>
    );
};
export default PrivacyForm;
