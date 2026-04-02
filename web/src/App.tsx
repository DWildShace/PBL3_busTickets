import { Route, Routes } from "react-router-dom";
import ScreenMain from "./screens/Main";

export default function App() {
    return (
        <>
            <Routes>
                <Route path="/*" element={<ScreenMain />} />
            </Routes>
        </>
    );
}
