import {
    Box,
    Flex,
    Grid,
    Container,
    Heading,
    Text,
    Button,
    Card,
    Inset,
    TextField,
    Tabs,
    Badge,
    Avatar,
    Link,
} from "@radix-ui/themes";
import { MapPin, Calendar, Search, Ticket, Headset, Star } from "lucide-react";

const PROMOS = [
    {
        id: 1,
        title: "Giảm 20% cho khách hàng mới",
        code: "NEW20",
        image: "https://images.unsplash.com/photo-1544620347-c4fd4a3d5957?auto=format&fit=crop&q=80&w=600&h=300",
        validUntil: "30/11/2026",
    },
    {
        id: 2,
        title: "Flash Sale tuyến Sài Gòn - Đà Lạt",
        code: "DALAT50K",
        image: "https://images.unsplash.com/photo-1596422846543-75c6fc197f07?auto=format&fit=crop&q=80&w=600&h=300",
        validUntil: "15/12/2026",
    },
    {
        id: 3,
        title: "Ưu đãi khứ hồi tiết kiệm 100k",
        code: "KHUHOI100",
        image: "https://images.unsplash.com/photo-1520106208823-441ce4e21d33?auto=format&fit=crop&q=80&w=600&h=300",
        validUntil: "Hết hạn trong 3 ngày",
    },
];

const POPULAR_ROUTES = [
    {
        id: 1,
        from: "Sài Gòn",
        to: "Đà Lạt",
        price: "250.000đ",
        oldPrice: "350.000đ",
        image: "https://images.unsplash.com/photo-1625642471723-12744e6e42fd?auto=format&fit=crop&q=80&w=400&h=300",
    },
    {
        id: 2,
        from: "Hà Nội",
        to: "Sapa",
        price: "300.000đ",
        oldPrice: "400.000đ",
        image: "https://images.unsplash.com/photo-1559592413-7cec4d0cae2b?auto=format&fit=crop&q=80&w=400&h=300",
    },
    {
        id: 3,
        from: "Sài Gòn",
        to: "Nha Trang",
        price: "220.000đ",
        oldPrice: "",
        image: "https://images.unsplash.com/photo-1582234372722-50d7ccc30ebd?auto=format&fit=crop&q=80&w=400&h=300",
    },
    {
        id: 4,
        from: "Đà Nẵng",
        to: "Hội An",
        price: "100.000đ",
        oldPrice: "150.000đ",
        image: "https://images.unsplash.com/photo-1555921015-5532091f6026?auto=format&fit=crop&q=80&w=400&h=300",
    },
];

function HeroAndSearch() {
    return (
        <Box position="relative" pb="9" style={{ backgroundColor: "var(--gray-2)" }}>
            <div
                className="relative flex items-center justify-center h-96 bg-cover bg-center"
                style={{
                    backgroundImage:
                        "linear-gradient(rgba(0, 0, 0, 0.5), rgba(0, 0, 0, 0.7)), url('https://images.unsplash.com/photo-1469854523086-cc02fe5d8800?auto=format&fit=crop&q=80&w=2000')",
                }}
            >
                <Container size="4" px="4" className="h-fit mb-16 flex-1">
                    <Flex direction="column" justify="center" align="center" className="h-full flex-1" gap="4" pt="6">
                        <Heading size={{ initial: "7", md: "8" }} align="center" style={{ color: "white" }}>
                            XeNhanh - Nền tảng đặt vé xe hàng đầu
                        </Heading>
                        <Text size="5" align="center" style={{ color: "var(--gray-a9)" }}>
                            Cam kết giữ chỗ 100% - Hoàn tiền nếu không có xe.
                        </Text>
                    </Flex>
                </Container>
            </div>

            <Container size="4" px="4" style={{ marginTop: "-64px", position: "relative", zIndex: 10 }}>
                <Card
                    size="3"
                    variant="surface"
                    className="pt-3!"
                    style={{ backgroundColor: "var(--color-panel-solid)", boxShadow: "var(--shadow-4)" }}
                >
                    <Tabs.Root defaultValue="oneway">
                        <Tabs.List size="2" mb="4" color="blue">
                            <Tabs.Trigger value="oneway">
                                <Flex align="center" gap="2">
                                    Một chiều
                                </Flex>
                            </Tabs.Trigger>
                            <Tabs.Trigger value="roundtrip">
                                <Flex align="center" gap="2">
                                    Khứ hồi
                                </Flex>
                            </Tabs.Trigger>
                        </Tabs.List>

                        <Box pt="2">
                            <Tabs.Content value="oneway">
                                <Grid columns={{ initial: "1", md: "4" }} gap="4" align="end">
                                    <Box>
                                        <Text as="div" size="2" weight="bold" mb="2" color="gray" highContrast>
                                            Nơi đi
                                        </Text>
                                        <TextField.Root size="3" placeholder="Nhập tỉnh, thành phố nơi đi">
                                            <TextField.Slot>
                                                <MapPin size={18} />
                                            </TextField.Slot>
                                        </TextField.Root>
                                    </Box>

                                    <Box>
                                        <Text as="div" size="2" weight="bold" mb="2" color="gray" highContrast>
                                            Nơi đến
                                        </Text>
                                        <TextField.Root size="3" placeholder="Nhập tỉnh, thành phố nơi đến">
                                            <TextField.Slot>
                                                <MapPin size={18} />
                                            </TextField.Slot>
                                        </TextField.Root>
                                    </Box>

                                    <Box>
                                        <Text as="div" size="2" weight="bold" mb="2" color="gray" highContrast>
                                            Ngày đi
                                        </Text>
                                        <TextField.Root size="3" type="date">
                                            <TextField.Slot>
                                                <Calendar size={18} />
                                            </TextField.Slot>
                                        </TextField.Root>
                                    </Box>

                                    <Button size="3" color="amber" variant="solid" style={{ cursor: "pointer" }}>
                                        <Search size={18} />
                                        Tìm chuyến xe
                                    </Button>
                                </Grid>
                            </Tabs.Content>

                            <Tabs.Content value="roundtrip">
                                <Grid columns={{ initial: "1", md: "5" }} gap="4" align="end">
                                    <Box>
                                        <Text as="div" size="2" weight="bold" mb="2" color="gray" highContrast>
                                            Nơi đi
                                        </Text>
                                        <TextField.Root size="3" placeholder="Điểm đi">
                                            <TextField.Slot>
                                                <MapPin size={18} />
                                            </TextField.Slot>
                                        </TextField.Root>
                                    </Box>

                                    <Box>
                                        <Text as="div" size="2" weight="bold" mb="2" color="gray" highContrast>
                                            Nơi đến
                                        </Text>
                                        <TextField.Root size="3" placeholder="Điểm đến">
                                            <TextField.Slot>
                                                <MapPin size={18} />
                                            </TextField.Slot>
                                        </TextField.Root>
                                    </Box>

                                    <Box>
                                        <Text as="div" size="2" weight="bold" mb="2" color="gray" highContrast>
                                            Ngày đi
                                        </Text>
                                        <TextField.Root size="3" type="date">
                                            <TextField.Slot>
                                                <Calendar size={18} />
                                            </TextField.Slot>
                                        </TextField.Root>
                                    </Box>

                                    <Box>
                                        <Text as="div" size="2" weight="bold" mb="2" color="gray" highContrast>
                                            Ngày về
                                        </Text>
                                        <TextField.Root size="3" type="date">
                                            <TextField.Slot>
                                                <Calendar size={18} />
                                            </TextField.Slot>
                                        </TextField.Root>
                                    </Box>

                                    <Button size="3" color="amber" variant="solid" style={{ cursor: "pointer" }}>
                                        <Search size={18} />
                                        Tìm
                                    </Button>
                                </Grid>
                            </Tabs.Content>
                        </Box>
                    </Tabs.Root>
                </Card>
            </Container>
        </Box>
    );
}

function Promotions() {
    return (
        <Box py="8" style={{ backgroundColor: "var(--color-background)" }}>
            <Container size="4" px="4">
                <Flex justify="between" align="end" mb="5">
                    <Heading size="6" weight="bold">
                        Ưu đãi nổi bật
                    </Heading>
                    <Link href="#" size="3" color="blue" weight="medium">
                        Xem tất cả
                    </Link>
                </Flex>

                <Grid columns={{ initial: "1", sm: "2", md: "3" }} gap="5">
                    {PROMOS.map((promo) => (
                        <Card
                            key={promo.id}
                            size="2"
                            variant="surface"
                            style={{ padding: 0, overflow: "hidden", cursor: "pointer" }}
                        >
                            <Inset clip="padding-box" side="top" pb="current">
                                <img
                                    src={promo.image}
                                    alt={promo.title}
                                    style={{
                                        display: "block",
                                        objectFit: "cover",
                                        width: "100%",
                                        height: 140,
                                    }}
                                />
                            </Inset>
                            <Flex direction="column" gap="3" p="4" style={{ height: "100%" }}>
                                <Heading size="4" weight="bold" style={{ flexGrow: 1 }}>
                                    {promo.title}
                                </Heading>
                                <Flex justify="between" align="center">
                                    <Badge color="orange" variant="soft" size="2">
                                        {promo.code}
                                    </Badge>
                                    <Text size="2" color="gray">
                                        {promo.validUntil}
                                    </Text>
                                </Flex>
                            </Flex>
                        </Card>
                    ))}
                </Grid>
            </Container>
        </Box>
    );
}

function PopularRoutes() {
    return (
        <Box py="8" style={{ backgroundColor: "var(--gray-2)" }}>
            <Container size="4" px="4">
                <Heading size="6" weight="bold" mb="5">
                    Tuyến đường phổ biến
                </Heading>

                <Grid columns={{ initial: "1", sm: "2", md: "4" }} gap="4">
                    {POPULAR_ROUTES.map((route) => (
                        <Card
                            key={route.id}
                            size="1"
                            variant="surface"
                            style={{ cursor: "pointer", padding: 0, overflow: "hidden" }}
                        >
                            <Inset clip="padding-box" side="top" pb="current">
                                <Box position="relative">
                                    <img
                                        src={route.image}
                                        alt={`${route.from} đi ${route.to}`}
                                        style={{
                                            display: "block",
                                            objectFit: "cover",
                                            width: "100%",
                                            height: 140,
                                        }}
                                    />
                                    <Box
                                        position="absolute"
                                        bottom="0"
                                        left="0"
                                        width="100%"
                                        p="3"
                                        style={{ background: "linear-gradient(to top, rgba(0,0,0,0.7), transparent)" }}
                                    >
                                        <Heading size="4" weight="bold" style={{ color: "white" }}>
                                            {route.from} - {route.to}
                                        </Heading>
                                    </Box>
                                </Box>
                            </Inset>
                            <Box p="3">
                                <Flex align="center" gap="3">
                                    <Text size="4" weight="bold" color="blue">
                                        {route.price}
                                    </Text>
                                    {route.oldPrice && (
                                        <Text size="2" color="gray" style={{ textDecoration: "line-through" }}>
                                            {route.oldPrice}
                                        </Text>
                                    )}
                                </Flex>
                            </Box>
                        </Card>
                    ))}
                </Grid>
            </Container>
        </Box>
    );
}

function Features() {
    return (
        <Box py="9" style={{ backgroundColor: "var(--color-background)", borderTop: "1px solid var(--gray-a4)" }}>
            <Container size="4" px="4">
                <Grid columns={{ initial: "1", md: "3" }} gap="8">
                    <Flex direction="column" align="center" gap="4">
                        <Avatar size="6" fallback={<Ticket size={32} />} color="blue" variant="soft" radius="large" />
                        <Heading size="4" align="center">
                            Đa dạng lựa chọn
                        </Heading>
                        <Text size="3" color="gray" align="center">
                            Hơn 2000+ nhà xe chất lượng cao với nhiều dòng xe đa dạng (Limousine, Giường nằm, Ghế ngồi)
                            đáp ứng mọi nhu cầu.
                        </Text>
                    </Flex>

                    <Flex direction="column" align="center" gap="4">
                        <Avatar size="6" fallback={<Star size={32} />} color="amber" variant="soft" radius="large" />
                        <Heading size="4" align="center">
                            Chất lượng đảm bảo
                        </Heading>
                        <Text size="3" color="gray" align="center">
                            Các nhà xe được đánh giá chân thực từ hàng triệu hành khách. Cam kết giữ chỗ 100%, hoàn tiền
                            nếu không có xe.
                        </Text>
                    </Flex>

                    <Flex direction="column" align="center" gap="4">
                        <Avatar size="6" fallback={<Headset size={32} />} color="green" variant="soft" radius="large" />
                        <Heading size="4" align="center">
                            Hỗ trợ 24/7
                        </Heading>
                        <Text size="3" color="gray" align="center">
                            Đội ngũ tổng đài viên chuyên nghiệp luôn sẵn sàng giải đáp thắc mắc và hỗ trợ khách hàng xử
                            lý mọi sự cố.
                        </Text>
                    </Flex>
                </Grid>
            </Container>
        </Box>
    );
}

export default function PageMainIndex() {
    return (
        <>
            <HeroAndSearch />
            <Promotions />
            <PopularRoutes />
            <Features />
        </>
    );
}
