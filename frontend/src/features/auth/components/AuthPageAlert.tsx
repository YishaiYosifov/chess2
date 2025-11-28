"use client";

import Card from "@/components/ui/Card";
import { ErrorCode } from "@/lib/apiClient";
import constants from "@/lib/constants";
import Cookies from "js-cookie";
import React, { useEffect, useState } from "react";

const AUTH_ERROR_MESSAGES: Partial<Record<ErrorCode, string>> = {
    [ErrorCode.AUTH_USER_BANNED]: "Your account has been banned.",
} as const;

const AuthPageAlert = () => {
    const [alert, setAlert] = useState<string>();

    useEffect(() => {
        const errorCode = Cookies.get(constants.COOKIES.AUTH_FAILURE) as
            | ErrorCode
            | undefined;
        if (!errorCode) return;
        Cookies.remove(constants.COOKIES.AUTH_FAILURE);

        const message =
            AUTH_ERROR_MESSAGES[errorCode] ??
            "Failed to log in, please try again.";
        setAlert(message);
    }, []);

    if (!alert) return null;

    return (
        <Card
            className="h-min bg-red-500 text-center text-lg text-balance text-black"
            data-testid="authPageAlert"
        >
            {alert}
        </Card>
    );
};
export default AuthPageAlert;
