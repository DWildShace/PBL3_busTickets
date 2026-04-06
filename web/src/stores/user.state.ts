import type { User } from "@/api";
import { makeAutoObservable, runInAction } from "mobx";

export type ICurrentUser = {
    id: string;
    email: string;
    currentUser: User;
};

export class UserStore {
    user: ICurrentUser | null = null;
    userToken: string | null = null;
    isLoading: boolean = true;
    error: string | null = null;

    constructor() {
        makeAutoObservable(this);
    }

    get isAuthenticated() {
        return !!this.user;
    }

    get displayName() {
        if (!this.user) return "common:guest";
        if (!this.user.currentUser?.fullName) {
            return this.user.email;
        }
        return this.user.currentUser.fullName || "";
    }

    get displayRole() {
        if (!this.user) return "common:guest";
        return (
            String(this.user.currentUser.role?.roleName).charAt(0).toUpperCase() +
            String(this.user.currentUser.role?.roleName).slice(1).toLowerCase()
        );
    }

    async checkAuth() {
        console.log("Checking auth...");
        if (localStorage.getItem("auth_token") === null) {
            runInAction(() => {
                this.isLoading = false;
                this.user = null;
            });
            return;
        }
        this.isLoading = true;
        this.error = null;

        // try {
        //     const f = await aClient.query({
        //         query: USER_GET_CURRENT,
        //         fetchPolicy: "network-only",
        //     });

        //     if (!f.data?.currentUser) {
        //         this.user = null;
        //         this.error = "Returned empty currentUser";
        //         return;
        //     }

        //     runInAction(() => {
        //         if (!f.data?.currentUser) return;
        //         this.user = {
        //             id: f.data?.currentUser.id || "",
        //             email: f.data?.currentUser.email || "",
        //             currentUser: f.data?.currentUser,
        //         };
        //         this.error = null;
        //         this.userToken = localStorage.getItem("auth_token");
        //     });
        // } catch (e: any) {
        // localStorage.removeItem("auth_token");
        // await aClient.clearStore();
        // runInAction(() => {
        //     this.user = null;
        //     this.error = e.message || "Unknown error";
        // });
        // } finally {
        // runInAction(() => {
        //     this.isLoading = false;
        // });
        // }
    }

    async login(token: string) {
        this.logout();
        localStorage.setItem("auth_token", token);
        await this.checkAuth();
    }

    async logout() {
        localStorage.removeItem("auth_token");
        runInAction(() => {
            this.user = null;
        });
    }
}
