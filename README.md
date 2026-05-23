# SensorX - Giải pháp Quản lý Cung ứng Thông minh

![Banner](https://img.shields.io/badge/SensorX-OpenSource-blue?style=for-the-badge&logo=github)
![License](https://img.shields.io/badge/License-MIT-green.svg?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Active-success?style=for-the-badge)

SensorX là một hệ thống quản lý chuỗi cung ứng hiện đại, tích hợp công nghệ AI để tối ưu hóa quy trình từ báo giá (RFQ) đến kho vận. Dự án được thiết kế theo kiến trúc Microservices mạnh mẽ, dễ dàng mở rộng và bảo trì.

## 🌟 Tính năng nổi bật

- **Quản lý RFQ & Báo giá:** Luồng xử lý chuyên nghiệp từ yêu cầu của khách hàng đến phản hồi của nhân viên.
- **Tối ưu hóa Kho vận:** Quản lý tồn kho, nhập xuất kho và điều chỉnh kho thông minh.
- **Phân tích AI:** Tích hợp trí tuệ nhân tạo để phân tích dữ liệu báo giá và dự báo nhu cầu.
- **Giao diện hiện đại:** Xây dựng trên Next.js 14+ với trải nghiệm người dùng tối ưu.
- **Kiến trúc Microservices:** Tách biệt Master, Data, Warehouse để đảm bảo tính sẵn sàng cao.

## 🚀 Cấu trúc dự án

Hệ sinh thái SensorX bao gồm các thành phần chính:
- **[SensorX.Master](https://github.com/SensorX-labs/SensorX.Master):** Core logic, quản lý người dùng và điều phối luồng công việc.
- **[SensorX.Data](https://github.com/SensorX-labs/SensorX.Data):** Dịch vụ quản lý dữ liệu danh mục và sản phẩm.
- **[SensorX.Warehouse](https://github.com/SensorX-labs/SensorX.Warehouse):** Dịch vụ quản lý kho chuyên sâu.
- **[sensorx-fe](https://github.com/SensorX-labs/sensorx-fe):** Giao diện web cho cả khách hàng và quản trị viên.

## 🛠 Cài đặt & Sử dụng

### Yêu cầu hệ thống
- .NET 8 SDK (cho Backend)
- Node.js 20+ (cho Frontend)
- PostgreSQL / SQL Server
- Docker (khuyên dùng)

### Các bước nhanh
1. Clone các repository cần thiết.
2. Cấu hình file `appsettings.json` hoặc `.env`.
3. Chạy các dịch vụ backend: `dotnet run`.
4. Khởi chạy frontend: `npm install && npm run dev`.

## 🤝 Đóng góp

Chúng tôi luôn hoan nghênh các đóng góp từ cộng đồng!
1. Fork dự án.
2. Tạo nhánh tính năng (`git checkout -b feature/AmazingFeature`).
3. Commit thay đổi (`git commit -m 'Add some AmazingFeature'`).
4. Push lên nhánh (`git push origin feature/AmazingFeature`).
5. Mở một Pull Request.

## 📝 Giấy phép

Phân phối dưới giấy phép MIT. Xem `LICENSE` để biết thêm thông tin.

---
*Dự án được phát triển với tâm huyết bởi đội ngũ SensorX Labs.*
