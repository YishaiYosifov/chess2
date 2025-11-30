import Card from "@/components/ui/Card";

export const metadata = { title: "Privacy Policy - Anarchy Chess" };

export default function TosPage() {
    return (
        <main className="mx-auto flex max-w-7xl flex-1 justify-center p-5">
            <Card className="flex-1 gap-5">
                <h1 className="text-center text-5xl">Privacy Policy</h1>

                <hr className="text-white/50" />

                <p>
                    At Anarchy Chess, your privacy is important to us. This
                    Privacy Policy explains what information we collect, how we
                    use it, and your choices regarding your data.
                </p>

                <section>
                    <h2 className="text-2xl">1. Information We Collect</h2>
                    <p>
                        When you use Anarchy Chess, we collect the information
                        necessary to operate the service safely and reliably:
                    </p>
                    <ul className="list-inside list-disc">
                        <li>
                            Account Information: If you sign up with Google, we
                            store your email address. If you sign up with
                            Discord, we store your Discord user ID.
                        </li>
                        <li>
                            Profile Information: Your username, avatar,
                            &quot;about me&quot; description, and other profile
                            details.
                        </li>
                        <li>
                            Gameplay Data: Information required to manage
                            matches, ratings, quests, and other features.
                        </li>
                        <li>
                            IP Address: Used initially to set your country flag;
                            you may change this later in your settings.
                        </li>
                        <li>
                            In-Game Chat: Messages are saved privately for
                            moderation purposes and are not publicly visible.
                        </li>
                        <li>
                            Cookies and Tracking: We do not use cookies for
                            advertising. Session cookies or similar technologies
                            may be used solely to keep you logged in or provide
                            basic site functionality.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">2. How We Use Your Information</h2>
                    <p>We use the information we collect to:</p>
                    <ul className="list-inside list-disc">
                        <li>
                            Operate, maintain, and improve the Anarchy Chess
                            service.
                        </li>
                        <li>
                            Prevent abuse, fraud, and violations of our rules.
                        </li>
                        <li>
                            Maintain a safe and secure community environment.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">3. Data Retention</h2>
                    <p>
                        All profile information is kept while your account is
                        active. If you choose to delete your account, your
                        profile data will be removed, but match history will be
                        retained to preserve game records. Technical logs may be
                        retained for a limited period for security and auditing
                        purposes.
                    </p>
                </section>

                <section>
                    <h2 className="text-2xl">
                        4. Data Sharing and Third Parties
                    </h2>

                    <ul className="list-inside list-disc">
                        <li>
                            We do not sell, trade, or otherwise share your
                            personal information with third parties. Your data
                            is used solely to operate and improve Anarchy Chess.
                        </li>
                        <li>
                            Legal Requirements: We may disclose information if
                            required by law, subpoena, or other legal process,
                            or if we believe in good faith that disclosure is
                            necessary to protect our rights, your safety, or the
                            safety of others.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">5. Children&apos;s Privacy</h2>
                    <p>
                        Anarchy Chess is not intended for children under 13
                        years of age. We do not knowingly collect personal
                        information from children under 13. If we become aware
                        that we have inadvertently collected information from a
                        child under 13, we will take steps to delete it
                        promptly.
                    </p>
                </section>

                <section>
                    <h2 className="text-2xl">6. Account Deletion</h2>
                    <p>
                        You may request the deletion of your account at any time
                        by contacting{" "}
                        <a
                            href="mailto:support@anarchychess.org"
                            className="text-link"
                        >
                            support@anarchychess.org
                        </a>
                        . Please note that your personal profile information
                        will be removed, but your game history will remain to
                        maintain accurate records.
                    </p>
                </section>

                <section>
                    <h2 className="text-2xl">7. Your Agreement</h2>
                    <p>
                        By using Anarchy Chess, you agree that the information
                        we collect will be stored and used as described above to
                        operate and improve the service, prevent abuse, and
                        maintain community safety.
                    </p>
                </section>

                <section>
                    <h2 className="text-2xl">8. Updates to This Policy</h2>
                    <p>
                        We may update this Privacy Policy periodically. We will
                        notify you of significant changes by posting the new
                        policy on our website or through other communication
                        channels. Your continued use of Anarchy Chess after
                        changes constitutes acceptance of the updated policy.
                    </p>
                </section>

                <section>
                    <h2 className="text-2xl">9. Contact Us</h2>
                    <p>
                        If you have questions about this Privacy Policy or your
                        data, you can reach us at{" "}
                        <a
                            href="mailto:support@anarchychess.org"
                            className="text-link"
                        >
                            support@anarchychess.org
                        </a>
                        .
                    </p>
                </section>

                <p className="text-text/70">Last Updated: November 30, 2025</p>
            </Card>
        </main>
    );
}
