import { screen, render } from "@testing-library/react";
import FreshAuthForm from "../FreshAuthForm";

vi.mock("@/components/contexts/AuthContext");
vi.mock("../CollapsibleForm");
vi.mock("formik");

describe("FreshAuthForm", () => {
    it("should render everything", () => {
        render(
            <FreshAuthForm>
                <input data-testid="testInput" />
            </FreshAuthForm>
        );

        expect(screen.getByTestId("testInput")).toBeInTheDocument();
        expect(screen.getByTestId("collapsibleForm")).toBeInTheDocument();

        expect(screen.queryByTestId("passwordModal")).not.toBeInTheDocument();
    });
});
