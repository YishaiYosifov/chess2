import { Button, Col, Row } from "react-bootstrap";

const SubmitRow = ({ disableSubmitOn, onCancel }) => {
    return (
        <Row className="mt-2 g-1">
            <Col sm="auto">
                <Button type="submit" variant="dark" disabled={disableSubmitOn}>
                    Save
                </Button>
            </Col>
            <Col sm="auto">
                <Button variant="dark" onClick={onCancel}>
                    Cancel
                </Button>
            </Col>
        </Row>
    );
};
export default SubmitRow;
