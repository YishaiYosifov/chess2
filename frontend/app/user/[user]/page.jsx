import { apiRequest, processGetBody } from "@/app/utils/common";
import classes from "./user.module.scss";

import RatingCard from "@/app/components/pages/user/RatingCard";
import GamesTable from "@/app/components/pages/user/GamesTable";
import Profile from "@/app/components/Profile";
import { notFound } from "next/navigation";

export const metadata = {
    title: "Chess 2 - User",
};

const UserPage = async ({ params: { user } }) => {
    const dateMonthAgo = new Date();
    dateMonthAgo.setMonth(dateMonthAgo.getMonth() - 1);
    const ratingSinceTimestamp = (dateMonthAgo / 1000) | 0;

    const [profileRequest, gamesRequest, ratingsRequest] = await Promise.all([
        apiRequest(
            `/profile/${user}/info?${processGetBody({
                include: ["username", "about"],
            })}`,
            {
                method: "GET",
                next: { revalidate: 60 },
            }
        ),
        apiRequest(`/profile/${user}/games`, {
            next: { method: "GET", revalidate: 60 },
        }),
        apiRequest(
            `/profile/${user}/ratings?${processGetBody({
                since: ratingSinceTimestamp,
            })}`,
            {
                method: "GET",
                next: { revalidate: 60 },
            }
        ),
    ]);
    if (profileRequest.status === 404) notFound();

    const [profile, games, ratings] = [
        await profileRequest.json(),
        await gamesRequest.json(),
        await ratingsRequest.json(),
    ];

    return (
        <>
            <section className="mb-5">
                <div className="container">
                    <div className="row">
                        <div className="col-11 col-md-10 col-lg-9 col-xl-7 mx-auto">
                            <Profile {...profile} />
                        </div>
                    </div>
                </div>
            </section>

            <section className={classes["section-table"]}>
                <div className="container-fluid">
                    <div className="row pt-3 pb-5 justify-content-center">
                        <GamesTable games={games} username={profile.username} />
                    </div>
                </div>
            </section>

            <section className={classes["section-rating"]}>
                <div className="container-fluid">
                    <div className="row justify-content-center py-5 gap-5">
                        {Object.keys(ratings).map((variant) => {
                            return (
                                <RatingCard
                                    key={variant}
                                    variant={variant}
                                    {...ratings[variant]}
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
