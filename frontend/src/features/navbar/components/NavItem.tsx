"use client";

import { twMerge } from "tailwind-merge";
import Link from "next/link";
import React from "react";

import type { PolymorphicProps } from "@/types/polymorphicProps";

interface NavItemOwnProps {
    icon?: React.ReactNode;
    isCollapsed?: boolean;
}

const NavItem = <TProps extends React.ElementType = typeof Link>({
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
            className={twMerge(
                "flex cursor-pointer items-center gap-4 transition-opacity hover:opacity-70",
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
