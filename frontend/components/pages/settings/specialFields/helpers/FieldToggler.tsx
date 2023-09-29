"use client";

import { InputGroup, OverlayTrigger, Tooltip } from "react-bootstrap";
import { BsPencilFill } from "react-icons/bs";

const FieldToggler = ({
    onToggle,
    error,
}: {
    onToggle: () => void;
    error: string | boolean;
}) => {
    return (
        <InputGroup.Text>
            <OverlayTrigger
                placement="top"
                overlay={error ? <Tooltip>{error}</Tooltip> : <></>}
            >
                <button
                    className="reset"
                    disabled={Boolean(error)}
                    onClick={onToggle}
                >
                    <BsPencilFill />
                </button>
            </OverlayTrigger>
        </InputGroup.Text>
    );
};
export default FieldToggler;
