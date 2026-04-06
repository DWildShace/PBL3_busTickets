import type { ReactNode } from "react";
import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Box, Button, Dialog, Flex, IconButton, Link, Separator, Tabs, Text, TextField } from "@radix-ui/themes";
import { ArrowLeft, KeyRound, LockKeyhole, Mail, UserRound, XIcon } from "lucide-react";
import { GoogleLogin } from "@react-oauth/google";
import { useThemeContext } from "@/controller/ThemeProvider";
import { cn } from "@/lib/utils";

type AuthView = "login" | "register" | "forgot";

type LoginDialogProps = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
};

export default function LoginDialog({ open, onOpenChange }: LoginDialogProps) {
    const { t } = useTranslation();
    const [view, setView] = useState<AuthView>("login");

    const meta = useMemo(() => {
        if (view === "register") {
            return {
                title: t("auth:register.title"),
                description: t("auth:register.description"),
                badge: t("auth:register.badge"),
            };
        }

        if (view === "forgot") {
            return {
                title: t("auth:forgot.title"),
                description: t("auth:forgot.description"),
                badge: t("auth:forgot.badge"),
            };
        }

        return {
            title: t("auth:login.title"),
            description: t("auth:login.description"),
            badge: t("auth:login.badge"),
        };
    }, [t, view]);

    const handleOpenChange = (nextOpen: boolean) => {
        onOpenChange(nextOpen);
        if (!nextOpen) {
            setView("login");
        }
    };

    return (
        <Dialog.Root open={open} onOpenChange={handleOpenChange}>
            <Dialog.Content maxWidth="520px" className="overflow-hidden p-0!">
                <Box className="bg-linear-to-br px-6 py-6 bg-gray-400/5 border-b border-gray-400/5">
                    <IconButton
                        size="2"
                        color="gray"
                        variant="ghost"
                        className="absolute! right-6!"
                        onClick={() => onOpenChange(false)}
                    >
                        <XIcon size={20} />
                    </IconButton>
                    <Dialog.Title>{meta.title}</Dialog.Title>
                    <Dialog.Description size="2" color="gray">
                        {meta.description}
                    </Dialog.Description>
                </Box>

                <Box className="px-6 pb-5">
                    {view === "forgot" ? (
                        <ForgotPasswordForm onBack={() => setView("login")} />
                    ) : (
                        <Tabs.Root
                            value={view}
                            onValueChange={(value) => setView(value as Exclude<AuthView, "forgot">)}
                        >
                            <Tabs.List size="2" color="blue" mb="5" className="w-full">
                                <Tabs.Trigger value="login" className="flex-1!">
                                    {t("auth:login.tab")}
                                </Tabs.Trigger>
                                <Tabs.Trigger value="register" className="flex-1!">
                                    {t("auth:register.tab")}
                                </Tabs.Trigger>
                            </Tabs.List>

                            <Tabs.Content value="login">
                                <LoginForm
                                    onForgotPassword={() => setView("forgot")}
                                    onSwitchToRegister={() => setView("register")}
                                />
                            </Tabs.Content>
                            <Tabs.Content value="register">
                                <RegisterForm onSwitchToLogin={() => setView("login")} />
                            </Tabs.Content>
                        </Tabs.Root>
                    )}
                </Box>
            </Dialog.Content>
        </Dialog.Root>
    );
}

function LoginForm({
    onForgotPassword,
    onSwitchToRegister,
}: {
    onForgotPassword: () => void;
    onSwitchToRegister: () => void;
}) {
    const [isLoading, setIsLoading] = useState(false);
    const { t } = useTranslation();
    const { mode } = useThemeContext();

    return (
        <Flex direction="column" gap="4">
            <div
                className={cn(
                    "flex justify-center items-center w-full gbtn",
                    isLoading && "opacity-50 pointer-events-none",
                )}
                style={{
                    colorScheme: "light",
                }}
            >
                <GoogleLogin
                    onSuccess={(credentialResponse) => {
                        // handleGoogleLogin(credentialResponse.credential!);
                    }}
                    onError={() => {
                        // setError((prev) => ({ ...prev, global: "auth:google_login_failed" }));
                    }}
                    theme={mode === 1 ? "filled_black" : "filled_blue"}
                    size="large"
                    width="100%"
                    useOneTap
                    text={"signin_with"}
                    logo_alignment="center"
                />
            </div>
            <Flex align="center" gap="3" className="w-full">
                <Separator size="4" className="flex-1" />
                <Text size="1" color="gray" weight="medium" className="uppercase!">
                    {t("common:or")}
                </Text>
                <Separator size="4" className="flex-1" />
            </Flex>
            <FieldLabel label={t("auth:fields.email")}>
                <TextField.Root size="3" placeholder={t("auth:login.placeholders.email")}>
                    <TextField.Slot>
                        <Mail size={16} />
                    </TextField.Slot>
                </TextField.Root>
            </FieldLabel>

            <FieldLabel label={t("auth:fields.password")}>
                <TextField.Root size="3" type="password" placeholder={t("auth:login.placeholders.password")}>
                    <TextField.Slot>
                        <LockKeyhole size={16} />
                    </TextField.Slot>
                </TextField.Root>
            </FieldLabel>

            <Flex justify="between" align="center" gap="3" wrap="wrap">
                <Link
                    size="1"
                    onClick={(e) => {
                        e.preventDefault();
                        onForgotPassword();
                    }}
                    href="#"
                >
                    {t("auth:forgot.cta")}
                </Link>
            </Flex>

            <Button size="3">{t("auth:login.submit")}</Button>

            <FooterHint>
                <Text size="2" color="gray">
                    {t("auth:login.switchPrompt")}
                </Text>
                <Link
                    size="2"
                    onClick={(e) => {
                        e.preventDefault();
                        onSwitchToRegister();
                    }}
                    href="#"
                >
                    {t("auth:register.tab")}
                </Link>
            </FooterHint>
        </Flex>
    );
}

function RegisterForm({ onSwitchToLogin }: { onSwitchToLogin: () => void }) {
    const { t } = useTranslation();
    const { mode } = useThemeContext();
    const [isLoading, setIsLoading] = useState(false);

    return (
        <Flex direction="column" gap="4">
            <div
                className={cn(
                    "flex justify-center items-center w-full gbtn",
                    isLoading && "opacity-50 pointer-events-none",
                )}
                style={{
                    colorScheme: "light",
                }}
            >
                <GoogleLogin
                    onSuccess={(credentialResponse) => {
                        setIsLoading(true);
                        // handleGoogleLogin(credentialResponse.credential!);
                    }}
                    onError={() => {
                        setIsLoading(false);
                        // setError((prev) => ({ ...prev, global: "auth:google_login_failed" }));
                    }}
                    theme={mode === 1 ? "filled_black" : "filled_blue"}
                    size="large"
                    width="100%"
                    useOneTap
                    text={"signup_with"}
                    logo_alignment="center"
                />
            </div>
            <Flex align="center" gap="3" className="w-full">
                <Separator size="4" className="flex-1" />
                <Text size="1" color="gray" weight="medium" className="uppercase!">
                    {t("common:or")}
                </Text>
                <Separator size="4" className="flex-1" />
            </Flex>
            <FieldLabel label={t("auth:fields.fullName")}>
                <TextField.Root size="3" placeholder={t("auth:register.placeholders.fullName")}>
                    <TextField.Slot>
                        <UserRound size={16} />
                    </TextField.Slot>
                </TextField.Root>
            </FieldLabel>

            <FieldLabel label={t("auth:fields.email")}>
                <TextField.Root size="3" placeholder={t("auth:register.placeholders.email")}>
                    <TextField.Slot>
                        <Mail size={16} />
                    </TextField.Slot>
                </TextField.Root>
            </FieldLabel>

            <FieldLabel label={t("auth:fields.password")}>
                <TextField.Root size="3" type="password" placeholder={t("auth:register.placeholders.password")}>
                    <TextField.Slot>
                        <LockKeyhole size={16} />
                    </TextField.Slot>
                </TextField.Root>
            </FieldLabel>

            <FieldLabel label={t("auth:fields.confirmPassword")}>
                <TextField.Root size="3" type="password" placeholder={t("auth:register.placeholders.confirmPassword")}>
                    <TextField.Slot>
                        <KeyRound size={16} />
                    </TextField.Slot>
                </TextField.Root>
            </FieldLabel>

            <Text size="2" color="gray">
                {t("auth:register.helper")}
            </Text>

            <Button size="3">{t("auth:register.submit")}</Button>

            <FooterHint>
                <Text size="2" color="gray">
                    {t("auth:register.switchPrompt")}
                </Text>
                <Link
                    size="2"
                    onClick={(e) => {
                        e.preventDefault();
                        onSwitchToLogin();
                    }}
                    href="#"
                >
                    {t("auth:login.tab")}
                </Link>
            </FooterHint>
        </Flex>
    );
}

function ForgotPasswordForm({ onBack }: { onBack: () => void }) {
    const { t } = useTranslation();

    return (
        <Flex direction="column" gap="4">
            <div>
                <Button size="1" color="gray" variant="ghost" onClick={onBack}>
                    <ArrowLeft size={16} />
                    {t("auth:forgot.back")}
                </Button>
            </div>

            <FieldLabel label={t("auth:fields.email")}>
                <TextField.Root size="3" placeholder={t("auth:forgot.placeholders.email")}>
                    <TextField.Slot>
                        <Mail size={16} />
                    </TextField.Slot>
                </TextField.Root>
            </FieldLabel>

            <Text size="2" color="gray">
                {t("auth:forgot.helper")}
            </Text>

            <Button size="3">{t("auth:forgot.submit")}</Button>
        </Flex>
    );
}

function FieldLabel({ label, children }: { label: string; children: ReactNode }) {
    return (
        <label>
            <Text as="div" size="2" mb="2" weight="bold">
                {label}
            </Text>
            {children}
        </label>
    );
}

function FooterHint({ children }: { children: ReactNode }) {
    return (
        <Flex
            justify="center"
            align="center"
            gap="1"
            wrap="wrap"
            className="rounded-2xl px-4 py-3"
            style={{ backgroundColor: "var(--gray-2)" }}
        >
            {children}
        </Flex>
    );
}
