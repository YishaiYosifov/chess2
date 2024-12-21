"use client";

import Card from "@/components/helpers/Card";
import { useAuthedProfile } from "@/hooks/useAuthed";
import { useId } from "react";

const ProfileSettings = () => {
    const { about } = useAuthedProfile();
    const aboutMeId = useId();

    return (
        <Card className="flex-col gap-3 text-lg">
            <section>
                <label htmlFor={aboutMeId}>About Me</label>
                <textarea
                    id={aboutMeId}
                    placeholder={about}
                    className="w-full rounded-md text-black"
                    maxLength={300}
                />
            </section>

            <section>
                <label htmlFor="#country">Country</label>
                <select className="w-full rounded-md text-black">
                    <option>test</option>
                </select>
            </section>
        </Card>
    );
};
export default ProfileSettings;
