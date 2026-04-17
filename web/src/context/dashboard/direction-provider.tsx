import { createContext, useContext, useEffect } from "react";
import { DirectionProvider as RdxDirProvider } from "@radix-ui/react-direction";

export type Direction = "ltr" | "rtl";

const DEFAULT_DIRECTION = "ltr";

type DirectionContextType = {
    defaultDir: Direction;
    dir: Direction;
    setDir: (dir: Direction) => void;
    resetDir: () => void;
};

const DirectionContext = createContext<DirectionContextType | null>(null);

export function DirectionProvider({ children }: { children: React.ReactNode }) {
    const dir = DEFAULT_DIRECTION;

    useEffect(() => {
        const htmlElement = document.documentElement;
        htmlElement.setAttribute("dir", dir);
    }, [dir]);

    const setDir = () => {};
    const resetDir = () => {};

    return (
        <DirectionContext.Provider
            value={{
                defaultDir: DEFAULT_DIRECTION,
                dir,
                setDir,
                resetDir,
            }}
        >
            <RdxDirProvider dir={dir}>{children}</RdxDirProvider>
        </DirectionContext.Provider>
    );
}

// eslint-disable-next-line react-refresh/only-export-components
export function useDirection() {
    const context = useContext(DirectionContext);
    if (!context) {
        throw new Error("useDirection must be used within a DirectionProvider");
    }
    return context;
}
