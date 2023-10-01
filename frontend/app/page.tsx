import Image from "next/image";

import styles from "./index.module.scss";

import logo from "@/public/assets/logo.svg";

export const metadata = {
    title: "Chess 2 - Home",
};

const IndexPage = () => {
    return (
        <h1 className="mt-5">
            test <Image src={logo} alt="logo" width={100} height={100} />
        </h1>
    );
};
export default IndexPage;
