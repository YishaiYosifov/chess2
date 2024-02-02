import { render, screen } from "@testing-library/react";
import CollapsibleForm from "../CollapsibleForm";
import userEvent from "@testing-library/user-event";

vi.mock("formik");

describe("CollapsibleForm", () => {
    const FormContents = () => <input data-testid="testFormInput" />;
    const label = "Test Label";
    const defaultValue = "default value";

    function renderCollapsibleForm(disabled: boolean = false) {
        return render(
            <CollapsibleForm
                label="Test Label"
                defaultValue="default value"
                disabled={disabled}
            >
                <FormContents />
            </CollapsibleForm>
        );
    }

    async function renderOpenedForm() {
        const user = userEvent.setup();
        const renderResults = renderCollapsibleForm();
        await user.click(screen.getByTestId("collapsibleFormEnable"));

        return { user, renderResults };
    }

    it("should render the field before the form", () => {
        renderCollapsibleForm();

        expect(screen.queryByTestId("collapsibleForm")).not.toBeInTheDocument();

        const fieldLabel = screen.getByTestId("formFieldLabel");
        const staticInput = screen.getByTestId<HTMLInputElement>(
            "collapsibleFormStaticInput"
        );
        expect(fieldLabel.textContent).toBe(label);
        expect(staticInput).toBeDisabled();
        expect(staticInput.value).toBe(defaultValue);
    });

    it("should open the form when clicking the edit button", async () => {
        await renderOpenedForm();

        expect(screen.queryByTestId("collapsibleForm")).toBeInTheDocument();
        expect(screen.queryByTestId("testFormInput")).toBeInTheDocument();
    });

    it("should close on cancel", async () => {
        const { user } = await renderOpenedForm();

        const cancelButton = screen.getByTestId("collapsibleFormCancel");
        await user.click(cancelButton);

        expect(screen.queryByTestId("collapsibleForm")).not.toBeInTheDocument();
    });
});
