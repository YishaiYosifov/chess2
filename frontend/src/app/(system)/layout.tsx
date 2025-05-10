import { ReactNode } from "react";

import "../globals.css";

const RootLayout = async ({ children }: { children: ReactNode }) => {
    return (
        <html lang="en" data-bs-theme="dark">
            <body className="bg-background">{children}</body>
        </html>
    );
};
export default RootLayout;
