import { Box, Button, Container, Flex, Grid, Heading, Link, Separator, Text } from "@radix-ui/themes";
import { BusFrontIcon } from "lucide-react";

export default function MainFooter() {
    return (
        <Box py="9" style={{ backgroundColor: "var(--gray-3)", borderTop: "1px solid var(--gray-a4)" }}>
            <Container size="4" px="4">
                <Grid columns={{ initial: "1", sm: "2", md: "4" }} gap="6" mb="6">
                    <Box>
                        <Flex align="center" gap="2" mb="4">
                            <Box p="1" style={{ backgroundColor: "var(--blue-9)", borderRadius: "var(--radius-2)" }}>
                                <BusFrontIcon size={20} color="white" />
                            </Box>
                            <Heading size="4" color="blue" weight="bold">
                                VéXeNhanh
                            </Heading>
                        </Flex>
                        <Text size="2" color="gray" as="p" mb="2">
                            Bản quyền © 2026 VéXeNhanh.
                        </Text>
                        <Text size="2" color="gray" as="p">
                            Giải pháp đặt vé xe khách số 1 Việt Nam.
                        </Text>
                    </Box>

                    <Flex direction="column" gap="3">
                        <Heading size="3" highContrast>
                            Về chúng tôi
                        </Heading>
                        <Link href="#" size="2" color="gray">
                            Phần mềm nhà xe
                        </Link>
                        <Link href="#" size="2" color="gray">
                            Trở thành đối tác
                        </Link>
                        <Link href="#" size="2" color="gray">
                            Quy chế hoạt động
                        </Link>
                    </Flex>

                    <Flex direction="column" gap="3">
                        <Heading size="3" highContrast>
                            Hỗ trợ
                        </Heading>
                        <Link href="#" size="2" color="gray">
                            Hướng dẫn đặt vé
                        </Link>
                        <Link href="#" size="2" color="gray">
                            Câu hỏi thường gặp
                        </Link>
                        <Link href="#" size="2" color="gray">
                            Chính sách bảo mật
                        </Link>
                    </Flex>

                    <Flex direction="column" gap="3">
                        <Heading size="3" highContrast>
                            Tải ứng dụng
                        </Heading>
                        <Button variant="outline" color="gray" size="2" style={{ justifyContent: "center" }}>
                            App Store
                        </Button>
                        <Button variant="outline" color="gray" size="2" style={{ justifyContent: "center" }}>
                            Google Play
                        </Button>
                    </Flex>
                </Grid>

                <Separator size="4" mb="5" color="gray" />

                <Text size="1" color="gray" align="center" as="div">
                    Công ty Cổ phần VéXeNhanh - Địa chỉ: 123 Đường Tôn Đức Thắng, Phường Bến Nghé, Quận 1, TP. Hồ Chí
                    Minh
                </Text>
            </Container>
        </Box>
    );
}
