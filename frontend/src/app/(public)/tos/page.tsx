import Card from "@/components/ui/Card";

export const metadata = { title: "Terms of Service - Anarchy Chess" };

const TosPage = () => {
    return (
        <main className="mx-auto flex max-w-7xl flex-1 justify-center p-5">
            <Card className="flex-1 gap-5">
                <h1 className="text-center text-5xl">Terms of Service</h1>

                <hr className="text-white/50" />

                <section>
                    <h2 className="text-2xl">1. Acceptance of Terms</h2>
                    <p>
                        By creating an account, accessing, or using the Anarchy
                        Chess website (&quot;Service&quot;), you
                        (&quot;User&quot;) agree to be bound by these Terms of
                        Service (&quot;Terms&quot;) and our Privacy Policy. If
                        you do not agree to these Terms, do not use the Service.
                    </p>
                </section>

                <section>
                    <h2 className="text-2xl">2. Description of Service</h2>
                    <p>
                        Anarchy Chess is an online chess platform featuring
                        custom pieces, rules and gameplay mechanics. The service
                        includes account creation, matchmaking, in-game chat,
                        ratings, quests, and other features.
                    </p>
                </section>

                <section>
                    <h2 className="text-2xl">3. User Accounts</h2>
                    <ul className="list-inside list-disc">
                        <li>
                            You may need to create an account to access certain
                            features.
                        </li>
                        <li>
                            Account registration requires accurate information.
                            You are responsible for maintaining the
                            confidentiality of your account credentials.
                        </li>
                        <li>
                            You may not share your account credentials or access
                            with anyone. Each account is intended for individual
                            use only.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">
                        4. User Conduct and Responsibilities
                    </h2>
                    <ul className="list-inside list-disc">
                        <li>
                            You agree to use the Service responsibly and
                            lawfully.
                        </li>
                        <li>
                            You may not engage in cheating, harassment, abuse,
                            or disruptive behavior.
                        </li>
                        <li>
                            You are responsible for all activity that occurs
                            under your account.
                        </li>
                        <li>
                            We reserve the right to moderate content and remove
                            material that violates our policies.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">
                        5. Service Limitations and Modifications
                    </h2>
                    <ul className="list-inside list-disc">
                        <li>
                            The Service may be unavailable temporarily due to
                            maintenance or technical issues.
                        </li>
                        <li>
                            We reserve the right to modify, suspend, or
                            discontinue any feature at any time without notice.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">6. Data Handling and Privacy</h2>
                    <ul className="list-inside list-disc">
                        <li>
                            We collect, store, and use personal information as
                            described in our Privacy Policy.
                        </li>
                        <li>
                            By using the Service, you consent to the collection
                            and use of your data in accordance with the Privacy
                            Policy.
                        </li>
                        <li>
                            We take reasonable measures to protect your data
                            from unauthorized access or disclosure.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">7. Intellectual Property</h2>
                    <ul className="list-inside list-disc">
                        <li>
                            All content, including game mechanics, graphics,
                            logos, and software, is owned by Anarchy Chess or
                            its licensors.
                        </li>
                        <li>
                            You may not reproduce, distribute, or create
                            derivative works without explicit permission.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">
                        8. Disclaimers and Limitation of Liability
                    </h2>
                    <ul className="list-inside list-disc">
                        <li>
                            The Service is provided &quot;as is&quot; without
                            warranties of any kind.
                        </li>
                        <li>
                            We are not liable for damages arising from use,
                            inability to use, or errors in the Service.
                        </li>
                        <li>
                            Users can play at their own risk; Gameplay outcomes
                            may vary.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">9. Termination</h2>
                    <ul className="list-inside list-disc">
                        <li>
                            We may suspend or terminate your account for
                            violations of these Terms or other policies.
                        </li>
                        <li>
                            You may request account deletion by contacting{" "}
                            <a
                                href="mailto:support@anarchychess.org"
                                className="text-link"
                            >
                                support@anarchychess.org
                            </a>
                            .
                        </li>
                        <li>
                            Termination does not remove your obligations under
                            these Terms or your game history, which may be
                            retained.
                        </li>
                    </ul>
                </section>

                <section>
                    <h2 className="text-2xl">10. Changes to Terms</h2>
                    <p>
                        We may update these Terms periodically. We will notify
                        you of significant changes by posting the new policy on
                        our website or through other communication channels.
                        Your continued use of the Service after changes
                        constitutes acceptance of the updated Terms.
                    </p>
                </section>

                <p className="text-text/70">Last Updated: November 30, 2025</p>
            </Card>
        </main>
    );
};
export default TosPage;
