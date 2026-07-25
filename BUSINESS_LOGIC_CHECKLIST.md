# Business Logic Errors Checklist

## Hệ thống Đặt Vé Xe Khách - Kiểm Tra Lỗi Nghiệp Vụ

Danh sách kiểm tra các lỗi nghiệp vụ phổ biến trong quá trình phát triển và bảo trì.

---

## 1. Quản Lý Vé & Đặt Chỗ (Booking & Ticket Management)

### 1.1 Tính Toán Giá Vé
- [ ] **Giá cơ bản được tính toán đúng** từ `Trip.BasePrice`
- [ ] **Phí bổ sung được áp dụng** (nếu có):
  - [ ] Phí cao tốc (toll fee)
  - [ ] Phí bảo hiểm (insurance)
  - [ ] Phí dịch vụ (booking fee)
  - [ ] Phí trạm dừng (stops)
- [ ] **Chiết khấu được tính** (discount codes, promo)
- [ ] **Thuế VAT được tính** nếu có
- [ ] **Tổng giá cuối cùng chính xác**: `BasePrice + Fees - Discount + Tax`
- [ ] **Giá hiển thị trên UI khớp với giá backend**
- [ ] **Giá tệp hóa đơn khớp với giá thanh toán**

### 1.2 Tình Trạng Ghế
- [ ] **Ghế bị khóa khi được booking** (Lock before payment)
- [ ] **Ghế được giải phóng nếu thanh toán fail** (Release on payment failure)
- [ ] **Ghế được xác nhận sau khi thanh toán thành công** (Confirm after payment)
- [ ] **Không cho phép double-booking cùng một ghế**
- [ ] **Không thể booking ghế đã bị khóa/đặt**
- [ ] **Cập nhật sơ đồ ghế real-time** khi có booking mới
- [ ] **Phân loại ghế đúng**: hàng 1, hàng 2, ghế vip, ghế thường, ...

### 1.3 Giới Hạn Booking
- [ ] **Không cho phép booking quá số lượng ghế trống**
- [ ] **Giới hạn số ghế tối đa** mỗi lần booking (nếu có)
- [ ] **Giới hạn số vé** cho một người dùng trên cùng chuyến (nếu quy định)

### 1.4 Thời Gian Booking
- [ ] **Không cho phép booking chuyến đã khởi hành**
- [ ] **Không cho phép booking chuyến < X giờ** (e.g., 24 giờ trước)
- [ ] **Thời gian hết hạn booking tính đúng**
- [ ] **Cut-off time được kiểm tra** trước khi xác nhận vé

---

## 2. Quản Lý Thanh Toán & Hoàn Tiền (Payment & Refund)

### 2.1 Thanh Toán
- [ ] **Thanh toán được xử lý đúng** (MoMo integration)
- [ ] **Trạng thái thanh toán cập nhật chính xác**: `Pending` → `Completed` hoặc `Failed`
- [ ] **Transaction ID được lưu lại** từ gateway
- [ ] **Không tạo vé nếu thanh toán chưa confirmed**
- [ ] **Thông báo thanh toán đúng cho người dùng**
- [ ] **Xử lý timeout thanh toán** (e.g., sau 30 phút chuyển sang Failed)

### 2.2 Hoàn Tiền
- [ ] **Chính sách hoàn tiền được kiểm tra** (refund deadline, penalty)
- [ ] **Hoàn tiền không vượt quá số tiền thanh toán gốc**
- [ ] **Tính phí hủy đúng**: `Refund Amount = Original Price - Cancellation Fee`
- [ ] **Hoàn tiền chỉ cho phép khi trip chưa khởi hành**
- [ ] **Không cho hoàn tiền lần 2** nếu đã hoàn rồi
- [ ] **Trạng thái vé chuyển sang `Cancelled`** sau khi hoàn tiền
- [ ] **Ghế được giải phóng** sau khi hủy vé
- [ ] **Ghi log chi tiết** về các giao dịch hoàn tiền

### 2.3 Doanh Thu & Thống Kê
- [ ] **Doanh thu chỉ tính các vé đã thanh toán thành công**
- [ ] **Hoàn tiền được trừ khỏi doanh thu** (nếu cần)
- [ ] **Commission tính đúng** cho BusAdmin
- [ ] **Số liệu doanh thu real-time chính xác**

---

## 3. Quản Lý Chuyến Xe (Trip Management)

### 3.1 Tạo & Cập Nhật Chuyến
- [ ] **Thời gian khởi hành > thời gian hiện tại**
- [ ] **Thời gian đến > thời gian khởi hành**
- [ ] **Trạm đón/trả hợp lệ** (tồn tại trong DB)
- [ ] **Xe được gán hợp lệ** (có sẵn, không đang chạy chuyến khác)
- [ ] **Số chỗ chuyến khớp với sơ đồ ghế của xe**
- [ ] **Giá vé hợp lý** (không âm, không quá cao)
- [ ] **Trạng thái chuyến cập nhật đúng**: `Scheduled` → `In-Progress` → `Completed` / `Cancelled`

### 3.2 Quản Lý Trạng Thái Chuyến
- [ ] **Không thể chỉnh sửa chuyến đang chạy hoặc đã hoàn thành**
- [ ] **Hủy chuyến phải hoàn tiền tất cả hành khách**
- [ ] **Thông báo hành khách khi chuyến bị hủy**
- [ ] **Chuyển xuất phát tự động sang `In-Progress`** khi đến giờ
- [ ] **Chuyện kết thúc tự động sau khi đến** (nếu tích hợp GPS)

### 3.3 Quản Lý Xe (Bus)
- [ ] **Xe không được gán 2 chuyến trùng thời gian**
- [ ] **Kiểm tra khoảng cách & thời gian đều dõ** giữa các chuyến (turnaround time)
- [ ] **Xe ở trạng thái `Active`** mới được dùng
- [ ] **Không được dùng xe bảo trì**
- [ ] **Sơ đồ ghế xe phù hợp** với số ghế khai báo
- [ ] **Cập nhật vị trí xe** nếu có GPS integration

---

## 4. Quản Lý Người Dùng & Xác Thực (User & Authentication)

### 4.1 Xác Thực
- [ ] **JWT token được tạo đúng** (không sai thời hạn, issuer, audience)
- [ ] **Token không bị lộ** trong logs hoặc response headers
- [ ] **Refresh token logic đúng** (nếu có)
- [ ] **Logout xóa token** hoặc đánh dấu invalid
- [ ] **Google OAuth callback xử lý đúng** (không tạo duplicate account)

### 4.2 Phân Quyền
- [ ] **User chỉ thấy dữ liệu của họ**
  - [ ] Hành khách chỉ thấy booking/vé của họ
  - [ ] BusAdmin chỉ thấy xe & chuyến của công ty họ
  - [ ] SysAdmin thấy toàn bộ hệ thống
- [ ] **Endpoint được bảo vệ đúng** (Policy check)
- [ ] **Không cho phép escalate quyền** (passenger → busadmin)
- [ ] **BusAdmin upgrade request được kiểm duyệt** bởi SysAdmin

### 4.3 Dữ Liệu Nhạy Cảm
- [ ] **Mật khẩu không lưu dạng plaintext** (bcrypt/hash)
- [ ] **Số điện thoại/email được che giấu** nếu hiển thị
- [ ] **Không expose internal ID** nếu không cần
- [ ] **Không log sensitive data** (passwords, tokens, card info)

---

## 5. Quản Lý Hóa Đơn & Chứng Chỉ (Invoice & Ticket Certificate)

### 5.1 Tạo Hóa Đơn
- [ ] **Hóa đơn được tạo sau khi thanh toán thành công**
- [ ] **Số hóa đơn duy nhất** (không trùng)
- [ ] **Hóa đơn có đầy đủ thông tin**: hành khách, chuyến, vé, giá, ngày giờ
- [ ] **Tính thuế & phí đúng** trong hóa đơn
- [ ] **Tổng tiền hóa đơn = tổng tiền thanh toán**

### 5.2 Vé & QR Code
- [ ] **Vé được tạo sau booking thành công**
- [ ] **QR code/reference number duy nhất** cho mỗi vé
- [ ] **QR code không thể fake** (encoded đủ thông tin)
- [ ] **Vé chỉ hiệu lực cho chuyến đó** (không dùng cho chuyến khác)
- [ ] **In vé (PDF) khớp với dữ liệu booking**

---

## 6. Đánh Giá & Review (Rating & Review)

### 6.1 Quy Tắc Review
- [ ] **Chỉ hành khách đã hoàn thành chuyến mới được review**
- [ ] **Một hành khách chỉ review 1 lần** cho mỗi chuyến
- [ ] **Review chỉ được phép sau khi chuyến kết thúc**
- [ ] **Không cho phép review quá hạn** (e.g., 30 ngày)

### 6.2 Rating
- [ ] **Rating từ 1-5 sao**
- [ ] **Tính điểm trung bình đúng** (trung bình cộng)
- [ ] **Hiển thị số review/đánh giá chính xác**
- [ ] **Review bị xóa không được tính vào điểm**

---

## 7. Phân Tích & Thống Kê (Analytics & Reporting)

### 7.1 Dashboard Metrics
- [ ] **Tổng doanh thu tính đúng** (không tính hoàn tiền / phí)
- [ ] **Tổng chuyến tính đúng** (không tính bị hủy, hoặc tính riêng)
- [ ] **Tổng hành khách tính đúng** (unique, không tính duplicate)
- [ ] **Tỷ lệ lấp đầy ghế tính đúng**: (total seats booked) / (total seats available)
- [ ] **Doanh thu theo ngày/tháng** tính chính xác
- [ ] **Top routes/popular trips** xác định đúng

### 7.2 Báo Cáo
- [ ] **Báo cáo doanh thu không bao gồm đơn bị hủy/hoàn tiền** (nếu cần)
- [ ] **Báo cáo chi phí/commission đúng**
- [ ] **Giá trị hoàn tiền được cập nhật real-time**
- [ ] **Xuất báo cáo (CSV/PDF) không bị lỗi encoding**

---

## 8. Kiểm Soát & Giám Sát (Control & Monitoring)

### 8.1 SysAdmin Powers
- [ ] **SysAdmin có thể tạm khóa user** (mà không xóa vĩnh viễn)
- [ ] **SysAdmin có thể xem log hoạt động** của bất kì user
- [ ] **SysAdmin có thể xem/sửa dữ liệu** của bất kì công ty
- [ ] **SysAdmin action được log** với timestamp & người thực hiện

### 8.2 Kiểm Soát BusAdmin
- [ ] **BusAdmin chỉ quản lý xe/chuyến của công ty họ**
- [ ] **BusAdmin không thể xem dữ liệu công ty khác**
- [ ] **BusAdmin không thể upgrade quyền cho chính họ**
- [ ] **Tất cả action của BusAdmin được log**

### 8.3 Giám Sát Gian Lận
- [ ] **Phát hiện booking bất thường** (quá nhiều vé 1 lúc từ 1 account)
- [ ] **Phát hiện payment fraud** (failed transactions quá nhiều)
- [ ] **Phát hiện manipulation** (hoàn tiền nửa vẹn, v.v.)

---

## 9. Tính Nhất Quán Dữ Liệu (Data Consistency)

### 9.1 Atomicity
- [ ] **Booking + Payment là transaction nguyên tử**
  - Nếu fail: không booking, không tạo vé, không tạo hóa đơn
- [ ] **Hoàn tiền + Giải phóng ghế là transaction**
- [ ] **Tạo chuyến + Cập nhật xe status là transaction**

### 9.2 Consistency
- [ ] **Tổng ghế booking ≤ tổng ghế xe**
- [ ] **Tổng tiền vé ≤ tổng tiền thanh toán**
- [ ] **Không có ghost bookings** (trong DB nhưng không trong payment gateway)
- [ ] **Trạng thái ghế consistent** với trạng thái booking

### 9.3 Cascading & Foreign Keys
- [ ] **Xóa công ty → xóa/deactivate tất cả chuyến**
- [ ] **Xóa xe → không thể (hoặc soft delete)**
- [ ] **Xóa user → anonymize hoặc soft delete**
- [ ] **Xóa chuyến → hoàn tiền tất cả vé**

---

## 10. Lỗi UI & UX (Frontend Logic Errors)

### 10.1 Tính Toán Giá Hiển Thị
- [ ] **Giá cơ bản hiển thị đúng** trên search results
- [ ] **Chi tiết giá breakdown** hiển thị chính xác
  - Base price: X đ
  - Fees: Y đ
  - Discount: -Z đ
  - **Total: tổng đúng**
- [ ] **Giá real-time update** khi chọn/bỏ chọn seats
- [ ] **Không hiển thị giá âm**
- [ ] **Giá hiển thị khớp với backend** (double-check API call)

### 10.2 Validation Form
- [ ] **Validate ngày pick-up > hôm nay**
- [ ] **Validate ngày drop-off ≥ pick-up**
- [ ] **Validate trạm đón ≠ trạm trả**
- [ ] **Không cho submit khi có lỗi**

### 10.3 Kế Hoạch Hành Trình
- [ ] **Hiển thị giờ đón/trả chính xác**
- [ ] **Tính thời gian hành trình đúng** (từ trip data)
- [ ] **Hiển thị thông tin xe**: biển số, loại, số ghế, amenities
- [ ] **Hiển thị thông tin tài xế** (nếu có): tên, hình ảnh, rating

---

## Quy Trình Test

### Mỗi lần merge:
1. [ ] Chạy unit tests (nếu có)
2. [ ] Chạy integration tests (database)
3. [ ] Test manual các flow chính:
   - [ ] Booking thành công → Vé + Hóa đơn
   - [ ] Hoàn tiền → Ghế giải phóng + Money back
   - [ ] BusAdmin tạo chuyến → Hiển thị trên search
4. [ ] Kiểm tra logs: không có lỗi, không log sensitive data
5. [ ] Kiểm tra analytics: số liệu chính xác

### Nếu phát hiện lỗi:
- [ ] Tạo test case phát hiện lỗi
- [ ] Fix code
- [ ] Verify test pass
- [ ] Add regression test

---

## Ghi Chú

- **Ngôn ngữ:** C# (backend), TypeScript/React (frontend)
- **Database:** PostgreSQL
- **Payment:** MoMo gateway
- **Cập nhật:** Thêm mục kiểm tra mới khi phát hiện lỗi

