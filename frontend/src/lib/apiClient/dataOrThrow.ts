import { notFound } from "next/navigation";

export default async function dataOrThrow<T>(
    promise: Promise<{
        error?: unknown;
        data?: T;
        response: Response;
    }>,
): Promise<T> {
    const { error, data, response } = await promise;
    if (!error && data !== undefined) return data;

    if (response.status === 404) notFound();
    else throw error;
}
