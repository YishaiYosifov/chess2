"use server";

import { redirect } from "next/navigation";

export async function navigate(url: string): Promise<void> {
    return redirect(url);
}
