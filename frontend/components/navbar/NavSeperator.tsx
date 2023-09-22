import classes from "./NavSeperator.module.scss";

const NavSeperator = () => {
    return (
        <>
            <span className={classes.desktop}>|</span>
            <hr className={classes.mobile}></hr>
        </>
    );
};
export default NavSeperator;
