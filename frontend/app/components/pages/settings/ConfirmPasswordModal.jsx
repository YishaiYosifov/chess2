import { Button, Form, InputGroup, Modal } from "react-bootstrap";
import { BsLockFill } from "react-icons/bs";

import { forwardRef, useImperativeHandle, useRef, useState } from "react";

const ConfirmPasswordModal = forwardRef(({ ...props }, ref) => {
    async function getPassword() {
        setIsShowing(true);
    }
    useImperativeHandle(ref, () => ({ getPassword }), []);

    const passwordRef = useRef();
    const onHide = () => onSubmit(passwordRef.current.value);
    const [isShowing, setIsShowing] = useState(false);

    return (
        <Modal
            {...props}
            centered
            show={isShowing}
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
                <InputGroup hasValidation>
                    <Form.Control
                        placeholder="Confirm Password"
                        aria-label="confirm password"
                        name="confirmPassword"
                    />
                    <InputGroup.Text>
                        <BsLockFill />
                    </InputGroup.Text>
                    <Form.Control.Feedback></Form.Control.Feedback>
                </InputGroup>
            </Modal.Body>
            <Modal.Footer>
                <Button>Cancel</Button>
                <Button>Submit</Button>
            </Modal.Footer>
        </Modal>
    );
});

export default ConfirmPasswordModal;
