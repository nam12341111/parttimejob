# JobStudents - Server

## Giới thiệu
**JobStudents** là nền tảng kết nối sinh viên với các công việc bán thời gian phù hợp, tích hợp **AI Chatbot** thông minh để hỗ trợ tìm kiếm việc làm và tư vấn nghề nghiệp.

Dự án được xây dựng theo kiến trúc **Clean Architecture**, đảm bảo tính mở rộng, bảo trì và hiệu năng cao.

---

## Công nghệ sử dụng

- **Framework**: .NET 9 (ASP.NET Core Web API)
- **Database**: SQL Server (Triển khai qua Entity Framework Core Code-First)
- **Kiến trúc**: Clean Architecture (Domain - Application - Infrastructure - API)
- **AI & LLM**: Microsoft Semantic Kernel, Ollama (hỗ trợ mô hình cục bộ như DeepSeek, Llama 3)
- **Other**: SignalR (Real-time), JWT Authentication, Serilog.

---

## Chức năng chính

### 1. Hệ thống việc làm
- Đăng tin tuyển dụng, quản lý ca làm việc (Job Shifts).
- Ứng tuyển và theo dõi trạng thái hồ sơ.
- Tìm kiếm việc làm nâng cao.

### 2. Trợ lý AI thông minh (AI Chatbot)
- **Tư vấn hướng nghiệp**: Phân tích hồ sơ sinh viên để gợi ý công việc.
- **Tìm việc qua chat**: Người dùng có thể hỏi "Tìm cho tôi việc làm thêm buổi tối ở Quận 1".
- **Kiểm tra trạng thái**: Tra cứu thông tin công ty, trạng thái xét duyệt hồ sơ.

### 3. Quản trị (Admin)
- Quản lý người dùng, công ty.
- Phê duyệt tin đăng.
- Xem báo cáo, thống kê.

---

## Hướng dẫn cài đặt & Chạy

### Yêu cầu
- .NET SDK 9.0 trở lên
- SQL Server

### Các bước chạy
1. Clone dự án.
2. Cấu hình chuỗi kết nối Database và AI trong `appsettings.Development.json`.
3. Chạy lệnh khởi tạo Database:
   ```bash
   dotnet ef database update --project PTJ.Infrastructure --startup-project PTJ.API
   ```
4. Build và chạy Server:
   ```bash
   dotnet build
   dotnet run --project PTJ.API
   ```
5. Truy cập Swagger UI tại: `http://localhost:8080/swagger` (hoặc cổng tương ứng).

---

## Cấu trúc dự án
- **PTJ.Domain**: Các Entities, Enums cốt lõi.
- **PTJ.Application**: Logic nghiệp vụ, Interfaces, DTOs.
- **PTJ.Infrastructure**: Triển khai Database, AI Services, Repositories.
- **PTJ.API**: Controllers, Endpoints, DI Configuration.
