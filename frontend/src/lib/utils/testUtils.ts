import { RenderResult, render, screen } from "@testing-library/react";
import userEvent, { UserEvent } from "@testing-library/user-event";
import { useRouter } from "next/navigation";
import { ReactElement } from "react";

export function mockRouter() {
    const router = {
        back: jest.fn(),
        forward: jest.fn(),
        refresh: jest.fn(),
        push: jest.fn(),
        replace: jest.fn(),
        prefetch: jest.fn(),
    };
    const routerMock = useRouter as jest.Mock;
    routerMock.mockImplementation(() => router);

    return router;
}

export type FormValues<T> = Partial<Record<keyof T, string>>;

/**
 * Fills a form with the specificed field values
 *
 * @param user - the user event to interact with the form
 * @param fieldValues - the field values to fill the form
 */
export async function fillForm<T extends Record<string, string>>(
    user: UserEvent,
    fieldValues: FormValues<T>
): Promise<void> {
    for (const [fieldName, value] of Object.entries(fieldValues)) {
        if (value) await user.type(screen.getByLabelText(fieldName), value);
    }
}

export async function submitForm(user: UserEvent): Promise<void> {
    await user.click(screen.getByTestId("submitForm"));
}

export async function renderForm<T extends Record<string, string>>(
    form: ReactElement,
    fieldValues: FormValues<T> = {},
    doSubmit: boolean = true
): Promise<RenderResult> {
    const user = userEvent.setup();
    const renderResults = render(form);

    await fillForm(user, fieldValues);
    if (doSubmit) await submitForm(user);

    return renderResults;
}
