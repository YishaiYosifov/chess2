import { UserEvent } from "@testing-library/user-event";
import { screen } from "@testing-library/react";

import { ResponseError } from "@/client";

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
    fieldValues: FormFields<T>
): Promise<void> {
    for (const [fieldName, value] of Object.entries(fieldValues)) {
        await user.type(screen.getByLabelText(fieldName), value as string);
    }
}

export function submitForm(): void {
    screen.getByRole<HTMLFormElement>("form").requestSubmit();
}

/**
 * Creates a ResponseError instance
 */
export function responseErrFactory(
    body: BodyInit | null,
    response: ResponseInit = {}
): ResponseError {
    return new ResponseError(new Response(body, response));
}
