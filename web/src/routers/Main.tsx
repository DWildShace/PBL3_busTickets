import PageMainIndex from "@/pages/Main";
import { Route, Routes } from "react-router-dom";

export default function RouterMain() {
    return (
        <>
            <Routes>
                <Route index element={<PageMainIndex />} />
            </Routes>
        </>
    );
}
