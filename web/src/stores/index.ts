import { createContext, useContext } from "react";
import { UserStore } from "./user.state";

export class RootStore {
    user = new UserStore();
}

export const rootStore = new RootStore();
const StoreContext = createContext(rootStore);

export const useStore = () => useContext(StoreContext);
