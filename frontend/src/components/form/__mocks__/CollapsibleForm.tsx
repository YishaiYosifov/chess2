import { ReactNode } from "react";

// Convert collapsible form to a regular form when mocking
const CollapsibleFormMock = ({
    children,
    onSubmit,
}: {
    children: ReactNode;
    onSubmit: () => void;
}) => (
    <form onSubmit={onSubmit} data-testid="collapsibleForm">
        {children}

        <button type="submit" data-testid="collapsibleFormSubmit">
            Save
        </button>
        <button data-testid="collapsibleFormCancel">Cancel</button>
    </form>
);

export default CollapsibleFormMock;
