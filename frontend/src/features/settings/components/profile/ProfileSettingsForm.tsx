"use client";

import dynamic from "next/dynamic";

import TextField from "@/components/ui/TextField";
import Button from "@/components/ui/Button";
import Card from "@/components/ui/Card";
import { useAuthedUser } from "@/features/auth/hooks/useSessionUser";

const CountrySelector = dynamic(
    () => import("@/features/settings/components/CountrySelector"),
    { ssr: false },
);

const ProfileSettingsForm = () => {
    const user = useAuthedUser();
    console.log(user);

    return (
        <Card className="gap-5">
            <TextField label="Username" />
            <TextField
                label="About Me"
                as="textarea"
                className="min-h-60"
                maxLength={1000}
            />
            <CountrySelector />
            <Button>Save</Button>
        </Card>
    );
};
export default ProfileSettingsForm;
