import styles from "./SubmitRow.module.scss";

import { Button } from "react-bootstrap";

const SubmitField = ({
    disableSubmitOn,
    onCancel,
}: {
    disableSubmitOn: boolean;
    onCancel: () => void;
}) => {
    return (
        <div className={styles["submit-container"]}>
            <Button
                type="submit"
                variant="dark"
                disabled={Boolean(disableSubmitOn)}
            >
                Save
            </Button>

            <Button variant="dark" onClick={onCancel}>
                Cancel
            </Button>
        </div>
    );
};
export default SubmitField;
