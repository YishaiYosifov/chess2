"use client";

import { Button, Col, Row } from "react-bootstrap";
import Image from "next/image";

import { useRef, useState } from "react";

import classes from "./ChangeProfilePicture.module.scss";
import { apiRequest } from "@/lib/utils/common";
import { revalidateUser } from "@/app/actions";
import { useStore } from "@/app/store";

const ChangeProfilePicture = () => {
    const { username, pfpLastChanged, userId } = useStore.use.profile();

    const uploadPfpInput = useRef();
    const [lastChanged, setLastChanged] = useState(pfpLastChanged);
    const [status, setStatus] = useState("");

    const openFileSelector = () => uploadPfpInput.current.click();
    async function uploadPfp(event) {
        const data = new FormData();
        data.append("pfp", event.target.files[0]);

        const uploadResponse = await apiRequest(
            "/profile/upload-profile-picture",
            {
                body: data,
                method: "PUT",
            }
        );
        const uploadData = await uploadResponse.json();

        switch (uploadResponse.status) {
            case 201:
                setLastChanged(Date.now());
                setStatus("");
                revalidateUser(username);
                break;
            case 400:
                setStatus(uploadData.msg.pfp[0]);
                break;
            default:
                setStatus("Something went wrong.");
                console.error(uploadData);
                break;
        }
    }

    return (
        <Row className="align-items-center">
            <Col sm="auto">
                <Image
                    className={classes["profile-picture"]}
                    alt="profile picture"
                    src={`${process.env.NEXT_PUBLIC_API_URL}/api/profile/${userId}/profile-picture?${lastChanged}`}
                    width={120}
                    height={120}
                />
            </Col>
            <Col sm="auto">
                <input
                    type="file"
                    accept=".jpg,.jpeg,.png,.webp"
                    ref={uploadPfpInput}
                    onChange={uploadPfp}
                    hidden
                />

                <div>
                    <Button variant="dark" onClick={openFileSelector}>
                        Update Profile Picture
                    </Button>
                    <span className="ms-1 text-invalid">{status}</span>
                </div>
                <p className="mt-2">
                    Must be JPEG, PNG or WEBP and cannot exceed 2MB
                </p>
            </Col>
        </Row>
    );
};
export default ChangeProfilePicture;
