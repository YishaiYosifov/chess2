"use client";

import { PencilSquareIcon } from "@heroicons/react/24/outline";

import { useAuthedProfile } from "@/hooks/useAuthed";
import Input from "@/components/helpers/Input";
import Card from "@/components/helpers/Card";

const CredentialSettings = () => {
    const { username, email } = useAuthedProfile();

    function anonymiseEmail(email: string): string {
        const [emailUsername, domain] = email.split("@");
        const [second, top] = domain.split(".");
        return `${emailUsername[0]}****@${second[0]}***.${top}`;
    }

    return (
        <Card className="flex flex-col items-center gap-5 text-lg">
            <Input
                label="Username"
                placeholder={username}
                disabled
                className="w-full"
                icon={<PencilSquareIcon />}
            />
            <Input
                label="Email"
                disabled
                placeholder={anonymiseEmail(email)}
                className="w-full"
                icon={<PencilSquareIcon />}
            />
            <a className="text-secondary" href="">
                Change Password
            </a>
        </Card>
    );
};
export default CredentialSettings;
