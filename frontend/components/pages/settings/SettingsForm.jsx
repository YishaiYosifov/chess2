"use client";

import UsernameField from "./specialFields/UsernameField";
import EmailField from "./specialFields/EmailField";

const SettingsForm = ({ countries }) => {
    return (
        <>
            <div className="row g-3">
                <UsernameField />
                <EmailField />
            </div>
        </>
    );
};
export default SettingsForm;
