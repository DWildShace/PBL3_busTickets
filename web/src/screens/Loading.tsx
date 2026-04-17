import { Spinner, Text } from "@radix-ui/themes";
import { useTranslation } from "react-i18next";

export default function ScreenLoading() {
    const { t } = useTranslation("common");
    return (
        <div className="w-full h-dvh flex flex-col items-center justify-center">
            <Spinner size="3" className="scale-250" />
            <Text className="mt-8!" size="1" color="gray">
                {t("loading_dashboard")}
            </Text>
        </div>
    );
}
