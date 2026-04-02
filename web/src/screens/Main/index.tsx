import MainFooter from "@/components/Footer";
import MainHeader from "@/components/Header";
import RouterMain from "@/routers/Main";
import { Box } from "@radix-ui/themes";

export default function ScreenMain() {
    return (
        <>
            <Box
                style={{
                    display: "flex",
                    flexDirection: "column",
                    minHeight: "100vh",
                    backgroundColor: "var(--color-background)",
                }}
            >
                <MainHeader />

                <Box style={{ flexGrow: 1 }}>
                    <RouterMain />
                </Box>

                <MainFooter />
            </Box>
        </>
    );
}
