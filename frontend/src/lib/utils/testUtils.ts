import { ResponseError } from "@/client";
import { RenderResult, render, screen } from "@testing-library/react";
import userEvent, { UserEvent } from "@testing-library/user-event";
import { ReactElement } from "react";

type FormFields<T> = Partial<Record<keyof T, string>>;

/**
 * Fills a form with the specificed field values
 *
 * @param user - the user event to interact with the form
 * @param fieldValues - the field values to fill the form
 */
export async function fillForm<T>(
    user: UserEvent,
    fieldValues: FormFields<T>
): Promise<void> {
    for (const [fieldName, value] of Object.entries(fieldValues)) {
        await user.type(screen.getByLabelText(fieldName), value as string);
    }
}

export async function submitForm(user: UserEvent): Promise<void> {
    await user.click(screen.getByTestId("submitForm"));
}

interface RenderAndFillForm<T> {
    doSubmit?: boolean;
    fieldValues?: FormFields<T>;
}

/**
 * Renders a form
 *
 * @param form - the react form element to render
 * @param [options.doSubmit=true] - should the form be submitted after filling?
 * @param [options.fieldValues={}] - what should the form be filled with?
 * @returns a promise the resolves to the rendering results
 */
export async function renderAndFillForm<T extends FormFields<T>>(
    form: ReactElement,
    { doSubmit = true, fieldValues = {} }: RenderAndFillForm<T>
): Promise<RenderResult> {
    const user = userEvent.setup();
    const renderResults = render(form);

    await fillForm(user, fieldValues);
    if (doSubmit) await submitForm(user);

    return renderResults;
}

/**
 * Creates a form renderer function that has your form and fields as the default parameters.
 *
 * @param form - the react form element to render
 * @param defaultFields - the edfault values for the form fields
 * @returns a function that renders and fills the form with specificed options
 * @template T - the field values interface
 */
export const createFormRenderer =
    <T>(form: ReactElement, defaultFields: FormFields<T>) =>
    (options?: RenderAndFillForm<T>) =>
        renderAndFillForm(form, { fieldValues: defaultFields, ...options });

/**
 * Creates a ResponseError instance
 */
export function responseErrFactory(
    body: BodyInit | null,
    response: ResponseInit = {}
): ResponseError {
    return new ResponseError(new Response(body, response));
}
