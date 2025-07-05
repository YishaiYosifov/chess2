import {
    RenderOptions,
    RenderResult,
    render,
    screen,
} from "@testing-library/react";

import { UserEvent } from "@testing-library/user-event";
import { ReactElement } from "react";

import {
    AuthContext,
    AuthContextInterface,
} from "@/features/auth/contexts/authContext";
import { ApiProblemError, ApiProblemDetails } from "../apiClient";

type FormFields<T> = Partial<Record<keyof T, string>>;

/**
 * Fills a form with the specificed field values
 *
 * @param user - the user event to interact with the form
 * @param fieldValues - the field values to fill the form.
 * Name as a key, what to write as value
 */
export async function fillForm<T>(
    user: UserEvent,
    fieldValues: FormFields<T>,
): Promise<void> {
    for (const [fieldName, value] of Object.entries(fieldValues)) {
        await user.type(screen.getByLabelText(fieldName), value as string);
    }
}

export async function submitForm(user: UserEvent): Promise<void> {
    await user.click(screen.getByTestId("submitFormButton"));
}

/**
 * Render a component within the auth context
 *
 * @param ui - the element to render
 * @param contextOptions - what to pass to the context provider
 * @param renderOptions - options to pass to the render function
 */
export function renderWithAuthContext(
    ui: ReactElement,
    contextOptions: AuthContextInterface | undefined = {},
    renderOptions: RenderOptions = {},
): RenderResult {
    return render(
        <AuthContext.Provider value={contextOptions as AuthContextInterface}>
            {ui}
        </AuthContext.Provider>,
        renderOptions,
    );
}

/**
 * Creates a ResponseError instance
 */
export const problemDetailsFactory = (
    status: number,
    ...errors: ApiProblemError[]
): ApiProblemDetails => ({
    status: status,
    errors,
});
