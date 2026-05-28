# HƯỚNG DẪN CHI TIẾT VỀ FILE PROGRAM.CS - TRÁI TIM CỦA SERVER WEB API

File `Program.cs` là điểm khởi đầu duy nhất (Entry Point) của ứng dụng ASP.NET Core Web API. File này đảm nhận hai nhiệm vụ tối quan trọng:
1. **Đăng ký dịch vụ (Dependency Injection Container - DI):** Cung cấp các công cụ, dịch vụ, database context, cấu hình bảo mật... để toàn bộ hệ thống sử dụng.
2. **Thiết lập Đường ống dẫn yêu cầu (HTTP Request Pipeline - Middleware):** Định nghĩa cách một Request từ client đi qua các bộ lọc (Authentication, CORS, Routing) để đến được Controller và trả về Response.

---

## 1. Sơ Đồ Tổng Quan Về Vòng Đời Khởi Động & Luồng Xử Lý (Request Pipeline)

Dưới đây là sơ đồ mô tả cách một HTTP Request đi qua các lớp Middleware được định nghĩa trong `Program.cs`:

```text
[HTTP Request từ Client (React/Postman...)]
            │
            ▼
┌───────────────────────────────────────────────┐
│ 1. Exception Filter (Bắt lỗi toàn cục)        │
└───────────────────────┬───────────────────────┘
                        │ (Nếu có lỗi -> trả về 500/400 lập tức)
                        ▼
┌───────────────────────────────────────────────┐
│ 2. UseHttpsRedirection (Chuyển HTTP -> HTTPS) │
└───────────────────────┬───────────────────────┘
                        │
                        ▼
┌───────────────────────────────────────────────┐
│ 3. UseCors ("FrontendDev" - Cho phép React gọi)│
└───────────────────────┬───────────────────────┘
                        │ (Nếu sai Origin/Method -> Chặn CORS)
                        ▼
┌───────────────────────────────────────────────┐
│ 4. UseAuthentication (Đọc JWT Token trong Header)
└───────────────────────┬───────────────────────┘
                        │ (Giải mã Token -> Nạp danh tính vào HttpContext)
                        ▼
┌───────────────────────────────────────────────┐
│ 5. UseAuthorization (Kiểm tra quyền truy cập) │
└───────────────────────┬───────────────────────┘
                        │ (Nếu không có quyền -> Trả về 401/403)
                        ▼
┌───────────────────────────────────────────────┐
│ 6. MapControllers (Định tuyến Endpoint)      │
└───────────────────────┬───────────────────────┘
                        │ (Khớp URL với [Route] trong Controller)
                        ▼
           [Hàm Action trong Controller]
                        │
                        ▼
             [Tầng Services / Database]
                        │
                        ▼
           [HTTP Response trả về Client]
```

---

## 2. Giải Thích Chi Tiết Từng Khối Lệnh Trong `Program.cs`

Mã nguồn trong `Program.cs` được chia thành các phần chức năng rõ rệt sau:

### Khối 1: Khởi động và Nạp Biến Môi Trường
```csharp
Env.Load();
var builder = WebApplication.CreateBuilder(args);
```
* **`Env.Load()`**: Đọc file `.env` ở thư mục gốc của server để nạp các chuỗi kết nối bảo mật (Mật khẩu database, khóa JWT, cấu hình MoMo...) vào hệ thống.
* **`WebApplication.CreateBuilder(args)`**: Khởi tạo kiến trúc xây dựng Web API, tự động nạp cấu hình hệ thống từ `appsettings.json`, thiết lập bộ ghi log (Logging) và chuẩn bị Container chứa các dịch vụ.

---

### Khối 2: Đăng ký Dependency Injection (DI) cho các Services
```csharp
builder.Services.AddControllers(options => {
    options.Filters.Add<GlobalExceptionFilter>();
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITripSearchService, TripSearchService>();
...
```
* **`AddControllers(...)`**: Đăng ký các Controller vào hệ thống. Ở đây bạn sử dụng thêm `GlobalExceptionFilter` (Bộ lọc lỗi toàn cục). Nếu có bất kỳ lỗi không mong muốn nào xảy ra ở Tầng Service/Database mà bạn chưa bắt `try-catch`, bộ lọc này sẽ tự động bắt lấy và format lỗi trả về JSON chuẩn cho client, tránh bị sập server.
* **`AddHttpContextAccessor()`**: Cho phép các lớp Service bình thường có thể đọc được thông tin của HTTP Request hiện tại (ví dụ: lấy ID của user đang đăng nhập từ JWT).
* **`AddScoped<IService, Service>()`**: Đăng ký dịch vụ với vòng đời **Scoped** (Mỗi khi có 1 request HTTP gửi lên, Server sẽ tạo mới 1 instance của Service đó để xử lý, xử lý xong request thì tự động giải phóng). Đây là kiểu đăng ký tiêu chuẩn cho phần lớn các nghiệp vụ logic và Database Context.

---

### Khối 3: Cấu hình CORS (Cross-Origin Resource Sharing)
```csharp
builder.Services.AddCors(options => {
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:5173") // Cổng mặc định của Vite/React
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});
```
* **Ý nghĩa:** Trình duyệt web có cơ chế bảo mật ngăn chặn trang web ở tên miền này (`localhost:5173`) gọi API của tên miền khác (`localhost:5000`).
* **Giải pháp:** Đoạn mã này khai báo một chính sách tên là `"FrontendDev"` cho phép riêng cổng chạy của Frontend ReactJS được tự do gửi yêu cầu (POST, GET, PUT, DELETE) kèm Header lên Server API.

---

### Khối 4: Cấu hình Kết nối PostgreSQL nâng cao (Enum Mapping)
```csharp
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ...
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<UserRole>();
...
var dataSource = dataSourceBuilder.Build();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(dataSource)
);
```
* **Ánh xạ Enum (`MapEnum`)**: Đây là tính năng rất mạnh của nhà cung cấp driver PostgreSQL (`Npgsql`). Nó liên kết các Enum của C# (như `BookingStatus`, `UserRole`, `SeatType`) trực tiếp với kiểu dữ liệu `ENUM` đặc thù của Postgres trong Database, giúp tăng hiệu năng lưu trữ và kiểm soát tính toàn vẹn dữ liệu tốt hơn.
* **`AddDbContext`**: Đăng ký Database Context (`ApplicationDbContext`) sử dụng PostgreSQL qua Driver cấu hình phía trên.

---

### Khối 5: Cấu hình Bảo mật JWT (Authentication & Authorization)
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true, // Kiểm tra người phát hành token
            ValidateAudience = true, // Kiểm tra đối tượng sử dụng token
            ValidateIssuerSigningKey = true, // Kiểm tra chữ ký bảo mật
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)), // Khóa bí mật giải mã
            ValidateLifetime = true, // Kiểm tra thời hạn hết hạn
            ClockSkew = TimeSpan.Zero // Không cho phép lệch múi giờ khi check hết hạn
        };
    });
```
* **`AddAuthentication`**: Cấu hình xác thực bằng cơ chế **JWT (JSON Web Token)**. Mỗi khi client gửi request kèm Header `Authorization: Bearer <token>`, khối này sẽ tự động giải mã và kiểm tra token xem có bị giả mạo hay hết hạn hay không.
* **`AddAuthorization`**: Định nghĩa các chính sách phân quyền (Policy-based Authorization) dựa trên Vai trò (`Role`) của Token:
  * `"UserOnly"`: Yêu cầu quyền là Passenger, BusAdmin hoặc SysAdmin.
  * `"BusAdmin"`: Yêu cầu quyền là BusAdmin hoặc SysAdmin.
  * `"AdminOnly"`: Bắt buộc phải là Quản trị viên tối cao (SysAdmin).

---

### Khối 6: Cấu hình Swagger UI (Tài liệu API trực quan)
```csharp
builder.Services.AddSwaggerGen(options => {
    ...
    options.AddSecurityDefinition("Bearer", ...);
});
```
* Tạo ra trang tài liệu `/swagger` hiển thị toàn bộ các API hiện có để lập trình viên Frontend tiện theo dõi.
* Cấu hình thêm tính năng nhập Token `Bearer` trực tiếp trên giao diện Swagger để có thể test trực tiếp các API yêu cầu đăng nhập.

---

## 3. Khởi dựng ứng dụng và Thiết lập Thứ tự Middleware (Pipeline)

Sau dòng lệnh `var app = builder.Build();`, các dịch vụ đã được chuẩn bị xong, hệ thống tiến hành dựng đường ống dẫn Request:

* **`app.UseHttpsRedirection()`**: Tự động chuyển hướng các cuộc gọi từ cổng HTTP không bảo mật sang cổng HTTPS bảo mật.
* **`app.UseCors("FrontendDev")`**: Áp dụng luật CORS đã cấu hình ở Khối 3 để trình duyệt của client ReactJS không bị chặn kết nối.
* **`app.UseAuthentication()`**: Middleware giải mã và nhận diện danh tính người dùng qua JWT.
* **`app.UseAuthorization()`**: Middleware đối chiếu danh tính người dùng vừa giải mã được xem có khớp với chính sách (`[Authorize(Policy = "...")]`) ghi ở Controller hay không.
* **`app.MapControllers()`**: Khớp đường dẫn URL (Routing) của request để dẫn nó vào đúng Controller và hàm Action tương ứng.

---

## 4. Quản lý Database qua tham số dòng lệnh (CLI Flags)

Ở cuối file `Program.cs`, bạn thiết lập các cờ tiện ích để chạy bằng Command Line khi triển khai:

```csharp
if (args.Contains("--migrate"))
{
    await app.MigrateDatabaseAsync();
    return;
}

await app.InitializeDatabaseAsync();

if (args.Contains("--seed"))
{
    await app.SeedDatabaseAsync();
    return;
}

app.Run();
```

* **Chạy lệnh `dotnet run -- --migrate`**: Hệ thống sẽ tự động chạy toàn bộ các bản vá Database Migration để đồng bộ cấu hình bảng từ C# xuống Database rồi kết thúc ngay mà không cần bật server lên.
* **Chạy lệnh `dotnet run -- --seed`**: Server sẽ tự động nạp dữ liệu mẫu/dữ liệu khởi tạo (như tài khoản Admin mặc định, các tỉnh thành, tuyến đường cơ sở...) rồi tắt.
* **Chạy lệnh `dotnet run` thường**: Hệ thống khởi tạo Database cơ bản (`InitializeDatabaseAsync`) và chạy `app.Run()` để bật server Web API lên chờ lắng nghe kết nối từ các client.
