import Image from "next/image";

import classes from "./index.module.scss";

export const metadata = {
    title: "Chess 2 - Home",
};

const IndexPage = () => {
    return (
        <>
            <div className="container">
                <div className={`col ${classes.heading}`}>
                    <div className="row col-12">
                        <Image
                            src="/assets/logo.webp"
                            width={120}
                            height={120}
                            alt="website logo"
                        />
                        <h1>Chess 2</h1>
                    </div>
                </div>
            </div>
        </>
    );
};
export default IndexPage;
