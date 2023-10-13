import { notFound } from "next/navigation";

import { apiRequest, arrayToBody } from "@/lib/utils/common";
import styles from "./user.module.scss";

import RatingCard from "@/components/pages/user/RatingCard";
import GamesTable from "@/components/pages/user/GamesTable";
import Profile from "@/components/Profile";

import { Game, PublicProfile, Variants } from "@/lib/types";

export const metadata = {
    title: "Chess 2 - User",
};

export interface RatingArchive {
    achievedAt: number;
    elo: number;
}
export interface RatingArchiveData {
    archive: RatingArchive[];
    minRating: number;
    maxRating: number;
}

const UserPage = async ({ params: { user } }: { params: { user: string } }) => {
    // Find the date a month ago to fetch the ratings
    const dateMonthAgo = new Date();
    dateMonthAgo.setMonth(dateMonthAgo.getMonth() - 1);

    // Fetch everything
    const [profileRequest, gamesRequest, ratingsRequest] = await Promise.all([
        apiRequest(
            `/profile/${user}/info?include=${arrayToBody("include", [
                "username",
                "about",
                "pfpLastChanged",
            ])}`,
            {
                method: "GET",
                next: { revalidate: 60, tags: [`user-${user}`] },
            }
        ),

        apiRequest(`/profile/${user}/games`, {
            method: "GET",
            next: { revalidate: 60 },
        }),

        apiRequest(
            `/profile/${user}/ratings?since=${
                dateMonthAgo.toISOString().split("T")[0]
            }`,
            {
                method: "GET",
                next: { revalidate: 60 },
            }
        ),
    ]);
    if (profileRequest.status === 404) notFound();

    const [profile, games, ratings] = [
        (await profileRequest.json()) as PublicProfile,
        (await gamesRequest.json()) as Game[],
        (await ratingsRequest.json()) as {
            [key in Variants]: RatingArchiveData;
        },
    ];

    return (
        <>
            <section className="mb-5 mt-3">
                <div className="container">
                    <div className="row">
                        <div className="col-11 col-md-10 col-lg-9 col-xl-7 mx-auto">
                            <Profile profile={profile} />
                        </div>
                    </div>
                </div>
            </section>

            <section className={styles["section-table"]}>
                <div className="container-fluid">
                    <div className="row pt-3 pb-5 justify-content-center">
                        <GamesTable
                            games={games}
                            username={profile.username || ""}
                        />
                    </div>
                </div>
            </section>

            <section className={styles["section-rating"]}>
                <div className="container-fluid">
                    <div className="row justify-content-center py-5 gap-5">
                        {Object.keys(ratings).map((variant) => {
                            return (
                                <RatingCard
                                    key={variant}
                                    variant={variant}
                                    ratingData={ratings[Variants.Anarchy]}
                                />
                            );
                        })}
                    </div>
                </div>
            </section>
        </>
    );
};
export default UserPage;
