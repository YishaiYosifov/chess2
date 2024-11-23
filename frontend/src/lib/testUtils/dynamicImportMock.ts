import { DynamicOptions, Loader } from "next/dynamic";
import actualDynamic from "next/dynamic";
import React from "react";

let mockInitializers: (() => Promise<void> | undefined)[] = [];

export default async function preloadAll(): Promise<void> {
    if (!mockInitializers.length) return;

    // Copy and empty out `mockInitializers` right away so that any newly
    // enqueued components are found in the next pass.
    const initializers = mockInitializers.slice();
    mockInitializers = [];

    // While loading the components in this round of initializers, more
    // components may have been dynamically imported, adding more initializers
    // we should run and await.
    return Promise.all(initializers.map((preload) => preload())).then(
        preloadAll,
    );
}

type LoadableComponentType<TProps> = React.ComponentType<TProps> & {
    preload?: () => Promise<void>;
    render?: { preload?: () => Promise<void> };
};

vi.doMock("next/dynamic", async () => {
    const dynamicModule = await vi.importActual("next/dynamic");
    const dynamic = dynamicModule.default as typeof actualDynamic;

    const mockDynamic = <TProps>(
        loader: DynamicOptions<TProps> | Loader<TProps>,
        options?: DynamicOptions<TProps>,
    ): React.ComponentType<TProps> => {
        const LoadableComponent = dynamic(
            loader,
            options,
        ) as LoadableComponentType<TProps>;

        mockInitializers.push(() =>
            LoadableComponent.preload
                ? LoadableComponent.preload()
                : LoadableComponent?.render?.preload?.(),
        );

        return LoadableComponent;
    };

    return { default: mockDynamic };
});
