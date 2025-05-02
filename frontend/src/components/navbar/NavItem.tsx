"use client";

import React from "react";
import clsx from "clsx";
import Link from "next/link";

import type { PolymorphicProps } from "@/lib/polymorphicProps";

interface NavItemOwnProps {
    icon?: React.ReactNode;
    isCollapsed?: boolean;
}

const NavItem = <TProps extends React.ElementType>({
    icon,
    as,
    className,
    children,
    isCollapsed,
    ...props
}: PolymorphicProps<TProps, NavItemOwnProps>) => {
    const Component = as || Link;
    return (
        <Component
            {...(props as React.ComponentProps<TProps>)}
            className={clsx(
                "flex items-center gap-4 transition-opacity hover:opacity-70",
                className,
            )}
        >
            {icon && (
                <span className="size-9 cursor-pointer opacity-70">{icon}</span>
            )}
            {!isCollapsed && children}
        </Component>
    );
};

export default NavItem;
