import {
    Button,
    Card,
    Col,
    Form,
    InputGroup,
    OverlayTrigger,
    Row,
    Tooltip,
} from "react-bootstrap";
import { BsPencilFill } from "react-icons/bs";
import { Formik } from "formik";

import { useRef, useState } from "react";
import * as yup from "yup";

import { apiRequest } from "@/app/utils/common";
import { useStore } from "@/app/store";
import ConfirmPasswordModal from "../ConfirmPasswordModal";

const UsernameField = () => {
    const [isEditing, setIsEditing] = useState(false);
    const confirmPasswordModal = useRef();

    const schema = yup.object().shape({
        username: yup.string().username(),
    });
    const profile = useStore.use.profile();

    const timeSinceUsernameChange =
        (new Date() - new Date(profile.usernameLastChanged)) / 1000;
    const didUsernameChangeRecently =
        timeSinceUsernameChange < process.env.NEXT_PUBLIC_USERNAME_EDIT_EVERY;

    async function submitUsername(form, helpers) {
        await confirmPasswordModal.getPassword();
        /*const settingsResponse = await apiRequest("/profile/update-settings", {
            method: "PUT",
            json: form,
        });
        console.log(await settingsResponse.json());
        helpers.setSubmitting(false);*/
    }

    return (
        <>
            <ConfirmPasswordModal ref={confirmPasswordModal} />
            {isEditing ? (
                <Formik
                    validationSchema={schema}
                    initialValues={{ username: profile.username }}
                    onSubmit={submitUsername}
                >
                    {({
                        handleSubmit,
                        handleChange,
                        values,
                        errors,
                        isSubmitting,
                    }) => (
                        <Form noValidate onSubmit={handleSubmit}>
                            <Form.Group>
                                <Form.Label>Username</Form.Label>
                                <Form.Control
                                    aria-label="username"
                                    name="username"
                                    value={values.username}
                                    onChange={handleChange}
                                    isInvalid={errors.username}
                                />

                                <Form.Control.Feedback type="invalid">
                                    {errors.username}
                                </Form.Control.Feedback>
                            </Form.Group>

                            <Row className="mt-2 g-1">
                                <Col sm="auto">
                                    <Button
                                        type="submit"
                                        variant="dark"
                                        disabled={
                                            values.username ==
                                                profile.username || isSubmitting
                                        }
                                    >
                                        Save
                                    </Button>
                                </Col>
                                <Col sm="auto">
                                    <Button
                                        variant="dark"
                                        onClick={() => setIsEditing(false)}
                                    >
                                        Cancel
                                    </Button>
                                </Col>
                            </Row>
                        </Form>
                    )}
                </Formik>
            ) : (
                <Form.Group>
                    <Form.Label>Username</Form.Label>
                    <InputGroup>
                        <Form.Control
                            aria-label="username"
                            value={profile.username}
                            disabled
                        />

                        <InputGroup.Text>
                            <OverlayTrigger
                                placement="top"
                                overlay={
                                    didUsernameChangeRecently ? (
                                        <Tooltip id="tooltip-username-edit-error">
                                            Username changed too recently
                                        </Tooltip>
                                    ) : (
                                        <></>
                                    )
                                }
                            >
                                <button
                                    className="reset"
                                    disabled={didUsernameChangeRecently}
                                    onClick={() => setIsEditing(true)}
                                >
                                    <BsPencilFill />
                                </button>
                            </OverlayTrigger>
                        </InputGroup.Text>
                    </InputGroup>
                </Form.Group>
            )}
        </>
    );
};
export default UsernameField;
