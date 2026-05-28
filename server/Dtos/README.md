# TỔNG HỢP & PHÂN TÍCH HỆ THỐNG DTO (DATA TRANSFER OBJECTS)
Tài liệu này tổng hợp toàn bộ các DTO trong thư mục `server/Dtos` của dự án đặt vé xe khách PBL3. Các DTO được phân chia theo nhóm chức năng cốt lõi để dễ dàng theo dõi mục đích và cách chúng trao đổi dữ liệu giữa **Frontend (Client)**, **Backend (Server)** và **Database**.

---

## 💡 Khái Quát Cốt Lõi Về DTO Trong Dự Án
* **DTO Đầu Vào (`Request DTO` / `Query`):** Nhận dữ liệu gửi từ Client, chứa các annotation kiểm tra hợp lệ dữ liệu (như `[Required]`, `[EmailAddress]`, `[Range]`) để lọc sạch dữ liệu trước khi đi vào tầng logic nghiệp vụ.
* **DTO Đầu Ra (`Response DTO`):** Tổng hợp dữ liệu từ nhiều bảng cơ sở dữ liệu (Database Entities), chỉ giữ lại các trường an toàn và cần thiết cho giao diện Client, giúp tối ưu hiệu năng SQL và bảo mật thông tin nhạy cảm.

---

## 1. Nhóm Xác Thực & Tài Khoản (Auth & Profile)

Nhóm này phục vụ luồng đăng nhập, đăng ký và quản lý thông tin tài khoản người dùng hiện tại.

| File | Tên DTO | Vai Trò Cốt Lõi |
| :--- | :--- | :--- |
| **AuthDtos.cs** | `LoginRequestDto` | Nhận Email và Password từ client để thực hiện đăng nhập. |
| | `RegisterRequestDto` | Nhận thông tin đăng ký tài khoản mới (Email, Mật khẩu, Tên, SĐT, CCCD). |
| | `AuthResponseDto` | Trả về JWT Token, thời gian hết hạn và thông tin cơ bản của user sau khi đăng nhập thành công. |
| | `UserDto` | Trả về thông tin cơ bản của người dùng dạng rút gọn (không chứa mật khẩu). |
| | `OAuthGoogleRequestDto` | Nhận Google ID Token để xử lý đăng nhập bằng tài khoản Google. |
| **MeResponseDto.cs** | `MeResponseDto` | Trả về thông tin cá nhân đầy đủ của người đang đăng nhập (gồm thông tin User và thông tin Hành khách đi kèm). |
| | `MePassengerDto` / `MeUserInfoDto` | Các nhánh thông tin chi tiết (Hành khách, User, Vai trò) lồng trong `MeResponseDto`. |
| **Passenger.cs** | `UpdatePassengerDto` | Nhận thông tin cập nhật hồ sơ hành khách (Họ tên, SĐT, CCCD). |
| | `CreateBusAdminUpgradeRequestDto` | Nhận thông tin gửi yêu cầu nâng cấp tài khoản lên làm chủ nhà xe (Tên nhà xe, mã số thuế/giấy phép, hotline, lý do). |

---

## 2. Nhóm Tìm Kiếm & Chi Tiết Chuyến Xe (Search & Trip Details)

Nhóm này cung cấp bộ tìm kiếm chuyến xe thông minh theo tỉnh/hành trình, lọc dữ liệu nâng cao (theo giá, giờ chạy, nhà xe) và hiển thị sơ đồ ghế.

| File | Tên DTO | Vai Trò Cốt Lõi |
| :--- | :--- | :--- |
| **TripSearchDto.cs** | `TripSearchQuery` | Nhận thông tin tìm kiếm từ hành khách (Điểm đi, Điểm đến, Ngày đi, Sắp xếp, Bộ lọc khoảng giá/giờ chạy/tiện ích/nhà xe). |
| | `TripSearchResult` | Trả về danh sách chuyến xe khớp yêu cầu cùng thông tin tổng hợp bộ lọc động (tổng số xe, các mức giá, danh sách tiện ích hiện có). |
| | `TripSearchItemDto` | Đại diện cho một thẻ chuyến xe hiển thị trên trang tìm kiếm (Thời gian đi/đến, giá rẻ nhất, số ghế trống, tiện ích đi kèm). |
| **TripDetailDto.cs** | `TripDetailDto` | Trả về thông tin cực kỳ chi tiết của một chuyến xe được chọn (toàn bộ tiện ích, chính sách hoàn hủy, danh sách điểm đón/trả và sơ đồ ghế). |
| | `TripSeatDto` | Mô tả trạng thái một ghế trong sơ đồ (Vị trí X, Y, Tầng, Loại ghế, Ghế đó còn trống hay đã bị đặt). |
| | `TripDetailRouteStopDto` | Chi tiết một điểm đón hoặc trả dọc đường (Tên trạm, địa chỉ cụ thể, thứ tự dừng, thời gian dự kiến từ lúc xuất phát). |
| **AmenityDto.cs** | `AmenityDto` | Trả về danh sách tiện ích của xe (wifi, nước uống, khăn lạnh, cổng sạc USB) kèm icon hiển thị tương ứng. |
| **Province.cs** | `ProvinceResponse` | Trả về danh sách Tỉnh/Thành phố, Quận/Huyện, Phường/Xã có hỗ trợ tuyến đường (từ file json hoặc database địa lý). |

---

## 3. Nhóm Đặt Vé & Đơn Hàng Của Hành Khách (Booking & Orders)

Nhóm này xử lý quy trình chọn ghế, gửi thông tin liên hệ và xem lại vé xe đã đặt.

| File | Tên DTO | Vai Trò Cốt Lõi |
| :--- | :--- | :--- |
| **BookingDtos.cs** | `CreateBookingRequestDto` | Nhận thông tin đặt ghế từ khách (Mã chuyến xe, thông tin liên hệ, điểm đón/trả chọn trước, ID ghế đặt, phương thức thanh toán). |
| | `BookingResponseDto` | Trả về kết quả sau khi tạo đơn đặt vé (Tổng số tiền, trạng thái đơn, mã giao dịch thanh toán trực tuyến MoMo nếu có và danh sách vé). |
| | `BookingTicketDto` | Thông tin chi tiết của từng vé nằm bên trong đơn đặt hàng (Mã vé, số ghế, giá vé, lộ trình). |
| **MyOrdersDto.cs** | `MyOrdersResponseDto` | Trả về lịch sử mua vé của hành khách, tự động phân loại thành 3 danh sách: **Đã đặt (Sắp đi)**, **Hoàn thành (Đã đi)** và **Đã hủy**. |
| **TripReviewDtos.cs** | `CreateReviewDto` | Nhận đánh giá và bình luận của hành khách đối với chuyến đi đã hoàn thành (Số sao từ 1 đến 5, nội dung phản hồi). |
| | `TripReviewsResponseDto` | Trả về điểm đánh giá trung bình, tổng số lượt đánh giá và danh sách bình luận của hành khách cho chuyến xe đó. |

---

## 4. Nhóm Tích Hợp Thanh Toán Ví Điện Tử MoMo (Payments)

Nhóm này chịu trách nhiệm trao đổi dữ liệu với hệ thống cổng thanh toán MoMo.

| File | Tên DTO | Vai Trò Cốt Lõi |
| :--- | :--- | :--- |
| **PaymentDtos.cs** | `CreateMomoPaymentRequestDto` | Nhận yêu cầu tạo cổng thanh toán cho đơn hàng (`BookingId`). |
| | `CreateMomoPaymentResponseDto` | Trả về đường link thanh toán (`PayUrl`) và mã QR code để khách quét thanh toán bằng ví MoMo. |
| | `PaymentStatusDto` | Trả về trạng thái chi tiết của giao dịch (Chờ thanh toán, Thành công, Đã hủy, Số tiền, Thời gian thanh toán). |
| | `MomoReturnResultDto` | Nhận dữ liệu MoMo trả về trình duyệt của khách hàng sau khi thanh toán xong trên cổng MoMo. |
| | `MomoIpnRequestDto` | Nhận tín hiệu thanh toán ngầm (IPN) trực tiếp từ máy chủ MoMo gửi về máy chủ hệ thống để cập nhật trạng thái hóa đơn tự động và bảo mật. |

---

## 5. Nhóm Quản Lý Dành Cho Chủ Nhà Xe (Bus Admin / Partner)

Nhóm này cung cấp các chức năng nghiệp vụ quản lý đội xe, tuyến đường, chuyến đi và cập nhật hồ sơ nhà xe.

| File | Tên DTO | Vai Trò Cốt Lõi |
| :--- | :--- | :--- |
| **Bus.cs** | `CreateBusDto` / `UpdateBusDto` | Nhận thông tin thêm mới hoặc chỉnh sửa một xe khách cụ thể (Loại xe, Biển số xe, Trạng thái hoạt động). |
| **BusAdminManagement.cs** | `UpdateCompanyProfileDto` | Nhận thông tin cập nhật hồ sơ nhà xe (Tên nhà xe, Hotline, Hỗ trợ thanh toán trên xe hay không). |
| | `CreateTripDto` / `UpdateTripDto` | Nhận thông tin tạo hoặc chỉnh sửa chuyến đi (Tuyến đường, Xe phân công, Ngày khởi hành, Thời gian xuất phát/đến dự kiến). |
| | `CreateSeatLayoutDto` | Định nghĩa sơ đồ ghế mới cho loại xe (Nhãn ghế, Tầng, Loại ghế nằm/ngồi, tọa độ hiển thị X, Y trên lưới sơ đồ). |
| **CompanyProfileUpdateRequest.cs** | `CompanyProfileUpdateRequestDto` | Đại diện cho yêu cầu thay đổi thông tin nhà xe đang chờ Ban quản trị hệ thống phê duyệt. |

---

## 6. Nhóm Quản Lý Hệ Thống & Phân Tích (System Admin Management)

Nhóm này là xương sống cho Dashboard của Quản trị viên hệ thống (SysAdmin) để theo dõi doanh thu, phê duyệt nhà xe, quản lý giao dịch và duyệt hoàn tiền.

| File | Tên DTO | Vai Trò Cốt Lõi |
| :--- | :--- | :--- |
| **AdminDashboardDtos.cs** | `AdminDashboardOverviewDto` | Trả về toàn bộ dữ liệu thống kê tổng quát trên Dashboard Admin (KPIs doanh thu, số vé bán, lượng chuyến đi, biểu đồ doanh thu theo ngày/tháng, Top chặng bay đắt khách). |
| **AdminUsersDtos.cs** | `AdminUserListItemDto` / `AdminUsersListResponseDto` | Phục vụ chức năng quản lý danh sách người dùng (hiển thị số lượng đơn đặt vé, quyền hạn, trạng thái kích hoạt và hỗ trợ phân trang). |
| | `AdminCreateUserDto` / `AdminUpdateUserDto` | Nhận thông tin Admin thêm/sửa tài khoản trực tiếp trong hệ thống (Gán vai trò Admin, BusAdmin...). |
| **CompanyManagementDtos.cs** | `AdminCompanyListItemDto` | Danh sách nhà xe đối tác kèm theo các số liệu thống kê (Số lượng admin nhà xe, số tuyến đường, số xe, số chuyến đi đang chạy). |
| **BusAdminUpgradeRequest.cs**| `BusAdminUpgradeRequestListItemDto` | Hiển thị danh sách các đơn đăng ký mở nhà xe của người dùng gửi lên hệ thống kèm thông tin người duyệt và ghi chú kiểm duyệt. |
| **RefundManagementDtos.cs** | `RefundRequestListItemDto` | Quản lý các yêu cầu hoàn tiền vé của khách hàng (Mã đơn hàng, số tiền yêu cầu hoàn, lý do khách đưa ra, thông tin liên hệ). |
| | `ProcessRefundRequestDto` | Nhận ghi chú phê duyệt hoặc từ chối hoàn tiền từ Admin hệ thống. |
| **RevenueAnalyticsDtos.cs** | `RevenueAnalyticsDto` | Cung cấp dữ liệu báo cáo chuyên sâu về doanh thu (Doanh thu thực tế, biểu đồ tăng trưởng, thống kê doanh thu theo từng cổng thanh toán và từng nhà xe đối tác). |
| **ReviewManagementDtos.cs** | `ReviewListItemDto` / `ReviewDetailDto` | Danh sách và chi tiết các bình luận/đánh giá chuyến đi. Cho phép Admin ẩn các đánh giá spam, chứa từ ngữ thô tục hoặc gắn cờ cảnh báo (`IsFlagged`). |
| **TransactionDtos.cs** | `TransactionListItemDto` / `TransactionDetailDto` | Thống kê và chi tiết dòng tiền thanh toán trực tuyến (Mã giao dịch, trạng thái từ Momo, thông tin khách mua vé, chi tiết các vé được mua trong giao dịch đó). |
| **TripMonitoringDtos.cs** | `TripMonitoringListItemDto` | Giúp Admin theo dõi trực tiếp trạng thái vận hành của các chuyến đi đang hoạt động (Tuyến đường, Biển số xe, Tỷ lệ lấp đầy ghế, Doanh thu tạm tính của chuyến đi). |

---

*Tài liệu này được cập nhật tự động dựa trên cấu trúc các DTO trong thư mục dự án.*
