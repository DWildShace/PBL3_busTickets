import { useThemeContext } from "@/controller/ThemeProvider";
import { Box, Flex, Container, Heading, Button, Link, IconButton } from "@radix-ui/themes";
import { BusFront, ContrastIcon, Menu, Moon, Sun } from "lucide-react";

function MainHeader() {
    const { theme, toggleTheme } = useThemeContext();

    return (
        <Box py="3" className="sticky top-0 z-50 bg-(--color-panel-solid) backdrop-blur-sm border-b border-(--gray-a4)">
            <Container size="4" px="4">
                <Flex justify="between" align="center">
                    <Flex align="center" gap="2" className="cursor-pointer">
                        <IconButton variant="soft" color="blue" size="3">
                            <BusFront size={24} color="white" />
                        </IconButton>
                        <Heading size="5" color="blue" weight="bold">
                            XeNhanh
                        </Heading>
                    </Flex>

                    <Flex align="center" gap="5" display={{ initial: "none", md: "flex" }}>
                        <Link href="#" color="gray" size="3" weight="medium" highContrast className="no-underline!">
                            Quản lý đơn hàng
                        </Link>
                        <Link href="#" color="gray" size="3" weight="medium" highContrast className="no-underline!">
                            Mở bán vé trên XeNhanh
                        </Link>
                        <Link href="#" color="gray" size="3" weight="medium" highContrast className="no-underline!">
                            Trở thành đối tác
                        </Link>
                    </Flex>

                    <Flex align="center" gap="3">
                        <IconButton variant="soft" color="gray" size="2" onClick={toggleTheme}>
                            {theme === 1 ? (
                                <Sun size={18} />
                            ) : theme === 2 ? (
                                <Moon size={18} />
                            ) : (
                                <ContrastIcon size={18} />
                            )}
                        </IconButton>
                        <Button variant="solid" color="blue">
                            Đăng nhập
                        </Button>
                        <IconButton variant="ghost" color="gray" className="inline-flex md:hidden">
                            <Menu size={24} />
                        </IconButton>
                    </Flex>
                </Flex>
            </Container>
        </Box>
    );
}

export default MainHeader;
