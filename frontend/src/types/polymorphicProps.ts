import React from "react";

type PropsOf<C extends React.ElementType> = React.ComponentProps<C>;
export type PolymorphicProps<
    C extends React.ElementType,
    Props = object,
> = Props &
    Omit<PropsOf<C>, keyof Props> & {
        as?: C;
    };

export type PolymorphicRef<C extends React.ElementType> =
    React.ComponentPropsWithRef<C>["ref"];
