import { ReactNode } from "react";

const CollapsibleFormMock = ({ children }: { children: ReactNode }) => (
    <form data-testid="collapsibleForm">{children}</form>
);

export default CollapsibleFormMock;
