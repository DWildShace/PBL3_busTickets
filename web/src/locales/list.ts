import LocaleEng from "@/locales/resources/en/index";
import LocaleVie from "@/locales/resources/vi/index";

export type LocaleResourcesType = {
    [key: string]: {
        [key: string]: string;
    };
};

export const LocalesResources = {
    en: LocaleEng,
    vi: LocaleVie,
};

export const knownLocales: Record<
    string,
    {
        name: string;
        code: string;
        code4: string;
        code4under: string;
    }
> = {
    "en-US": {
        name: "English",
        code: "en",
        code4: "en-US",
        code4under: "en_US",
    },
    "vi-VN": {
        name: "Tiếng Việt",
        code: "vi",
        code4: "vi-VN",
        code4under: "vi_VN",
    },
};
