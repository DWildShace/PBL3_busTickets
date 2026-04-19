import { Route, Routes } from "react-router-dom";
import ScreenMain from "./screens/Main";
import { useEffect } from "react";
import { useStore } from "./stores";
import { observer } from "mobx-react-lite";
import RouterAdmin from "./routers/admin";

// eslint-disable-next-line react-refresh/only-export-components
const App = () => {
    const store = useStore();
    useEffect(() => {
        store.user.checkAuth();

        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <>
            <Routes>
                <Route path="/*" element={<ScreenMain />} />
                {store.user.isAuthenticated && store.user.user?.role.roleName === "SysAdmin" && (
                    <Route path="/admin/*" element={<RouterAdmin />} />
                )}
            </Routes>
        </>
    );
};

export default observer(App);
