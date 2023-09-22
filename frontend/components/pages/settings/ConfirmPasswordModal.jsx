"use client";

import { Button, Form, InputGroup, Modal } from "react-bootstrap";
import { BsLockFill } from "react-icons/bs";

import { forwardRef, useImperativeHandle, useRef, useState } from "react";

const ConfirmPasswordModal = forwardRef(({ ...props }, ref) => {
    const getPassword = () =>
        new Promise((resolve, reject) => {
            setHandleSubmit(() => () => resolve(passwordRef.current.value));
            sethandleCancel(() => () => reject());
            setIsShowing(true);
        });

    useImperativeHandle(ref, () => ({ getPassword }), []);

    const [handleSubmit, setHandleSubmit] = useState();
    const [handleCancel, sethandleCancel] = useState();

    const [isShowing, setIsShowing] = useState(false);
    const passwordRef = useRef();

    return (
        <Modal
            {...props}
            centered
            show={isShowing}
            onHide={handleCancel}
            size="lg"
            backdrop="static"
            aria-labelledby="confirm-password-title-modal"
        >
            <Modal.Header closeButton>
                <Modal.Title id="confirm-password-title-modal">
                    Confirm Password
                </Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <InputGroup>
                    <Form.Control
                        placeholder="Confirm Password"
                        aria-label="confirm password"
                        name="confirmPassword"
                        ref={passwordRef}
                    />
                    <InputGroup.Text>
                        <BsLockFill />
                    </InputGroup.Text>
                </InputGroup>
            </Modal.Body>
            <Modal.Footer>
                <Button onClick={handleCancel}>Cancel</Button>
                <Button onClick={handleSubmit}>Submit</Button>
            </Modal.Footer>
        </Modal>
    );
});

export default ConfirmPasswordModal;
