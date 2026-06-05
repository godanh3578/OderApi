# Order & Sales Service - Microservices

**Dịch vụ quản lý bán hàng trung tâm trong kiến trúc Microservices**

## Mô tả

Order & Sales Service là dịch vụ trung tâm chịu trách nhiệm quản lý toàn bộ hoạt động bán hàng của doanh nghiệp. Service này:

- Tiếp nhận yêu cầu tạo đơn hàng từ nhân viên bán hàng
- Kiểm tra thông tin sản phẩm và tồn kho từ Product & Inventory Service
- Thực hiện tính toán giá trị đơn hàng, chiết khấu, công nợ
- Lưu trữ lịch sử giao dịch chi tiết
- Phát hành sự kiện `order.created` để các dịch vụ khác tiếp nhận

## Chức năng chính

✅ Quản lý đơn bán hàng  
✅ Module Sales (checkout, tính tiền, chiết khấu)  
✅ Module Payment & Debt  
✅ Quản lý khách hàng (CRM cơ bản)  
✅ Quản lý nhà cung cấp  
✅ Quản lý công nợ khách hàng  
✅ Áp dụng giảm giá và chiết khấu  
✅ Kiểm tra tình trạng tồn kho trước khi bán  
✅ Theo dõi lịch sử giao dịch  
✅ Phát hành sự kiện order.created  
✅ Hỗ trợ thống kê doanh số bán hàng  

## Cơ sở dữ liệu

**Database:** OrderDB

**Bảng dữ liệu chính:**

| Bảng | Mô tả |
|------|-------|
| Customers | Thông tin khách hàng |
| Suppliers | Thông tin nhà cung cấp |
| Orders | Đơn bán hàng |
| OrderDetails | Chi tiết sản phẩm trong đơn hàng |
| Payments | Lịch sử thanh toán |
| Debts | Công nợ khách hàng |
| ProductStockCaches | Cache tồn kho từ `stock.updated` |
| OutboxMessages | Hàng đợi event `order.created` |

## Công nghệ sử dụng

- **Framework:** ASP.NET Core 8.0 Web API
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Authentication:** JWT Bearer Token
- **API Documentation:** Swagger/OpenAPI
- **Containerization:** Docker
- **Event Bus:** RabbitMQ (Event Communication)

## API Endpoints

### Sales
- `POST /api/sales/checkout` - Hoàn tất giao dịch bán hàng
- `POST /api/sales/calculate-total` - Tính tổng tiền
- `POST /api/sales/apply-discount` - Áp dụng chiết khấu cho đơn hàng đã tạo

### Orders
- `GET /api/orders` - Danh sách đơn hàng (tìm kiếm/phân trang: `?search=&page=1&pageSize=20`)
- `GET /api/orders/{id}` - Chi tiết đơn hàng
- `POST /api/orders` - Tạo đơn hàng
- `PUT /api/orders/{id}/status` - Cập nhật trạng thái
- `PUT /api/orders/{id}/cancel` - Hủy đơn hàng
- `DELETE /api/orders/{id}` - Xóa đơn hàng (Admin)

### Customers
- `GET /api/customers` - Danh sách khách hàng
- `GET /api/customers/{id}` - Chi tiết khách hàng
- `GET /api/customers/{id}/purchase-history` - Lịch sử mua hàng
- `GET /api/customers/{id}/debts` - Công nợ khách hàng
- `POST /api/customers` - Tạo khách hàng
- `PUT /api/customers/{id}` - Cập nhật khách hàng
- `DELETE /api/customers/{id}` - Xóa khách hàng (Admin)

### Suppliers
- `GET /api/suppliers` - Danh sách nhà cung cấp
- `GET /api/suppliers/{id}` - Chi tiết nhà cung cấp
- `POST /api/suppliers` - Tạo nhà cung cấp
- `PUT /api/suppliers/{id}` - Cập nhật nhà cung cấp
- `DELETE /api/suppliers/{id}` - Xóa nhà cung cấp (Admin)

### Payments
- `GET /api/payments` - Lịch sử thanh toán
- `GET /api/payments/{id}` - Chi tiết thanh toán
- `POST /api/payments` - Ghi nhận thanh toán
- `GET /api/payments/order/{orderId}` - Thanh toán theo đơn

### Debts
- `GET /api/debts` - Danh sách công nợ
- `GET /api/debts/{id}` - Chi tiết công nợ
- `GET /api/debts/customer/{customerId}` - Công nợ theo khách
- `POST /api/debts/{id}/pay` - Ghi nhận trả nợ
- `PUT /api/debts/{id}/status` - Cập nhật trạng thái công nợ

## Kiến trúc Microservices

Order & Sales Service đóng vai trò **trung tâm xử lý nghiệp vụ bán hàng**:

```
┌─────────────────────────────────────────────────────────┐
│         Order & Sales Service                           │
│  (Quản lý bán hàng, đơn hàng, khách hàng, công nợ)    │
└──────────┬──────────────────────────┬──────────────────┘
           │                          │
           ▼                          ▼
   ┌──────────────────┐      ┌──────────────────┐
   │ Product &        │      │ User & Report    │
   │ Inventory        │      │ Service          │
   │ Service          │      │                  │
   │                  │      │ (Báo cáo, Thống │
   │ (Kiểm tra tồn    │      │  kê doanh số)    │
   │  kho, Sản phẩm)  │      │                  │
   └──────────────────┘      └──────────────────┘
```

## Quá trình hoạt động

1. **Tiếp nhận đơn hàng** → Nhân viên bán hàng gửi yêu cầu tạo đơn hàng
2. **Kiểm tra dữ liệu** → Xác thực khách hàng, sản phẩm, tồn kho
3. **Tính toán** → Tính giá trị, chiết khấu, công nợ
4. **Lưu trữ** → Ghi lại đơn hàng và chi tiết vào OrderDB
5. **Phát hành sự kiện** → Gửi event `order.created` qua RabbitMQ
6. **Báo cáo** → Các dịch vụ khác tiếp nhận để cập nhật báo cáo, thống kê

## Cài đặt

### Yêu cầu
- .NET 8.0 SDK
- SQL Server 2019 hoặc cao hơn
- RabbitMQ
- Node.js 18+ (cho Frontend)

### Khởi chạy

**Docker Compose (khuyến nghị):**
```bash
docker compose up --build
```

Lệnh này khởi chạy Order API, SQL Server và RabbitMQ.

**Backend API:**
```bash
cd OrderApi
dotnet run
```

API sẽ chạy tại: `http://localhost:5002`  
Swagger UI: `http://localhost:5002/swagger`

**Frontend:**
```bash
cd frontend
npm install
node server.js
```

Frontend sẽ chạy tại: `http://localhost:5173`

Tùy chọn cấu hình frontend API bằng biến môi trường:
```bash
VITE_API_URL=http://localhost:5002 npm run dev
```

## Xác thực

API sử dụng JWT Bearer Token. Thêm header vào request:
```
Authorization: Bearer <token>
```

## Cấu hình

Xem file `appsettings.json` để cấu hình:
- Kết nối Database
- JWT Secret Key
- CORS Origins
- RabbitMQ Host

## Sự kiện RabbitMQ

**Event phát hành:**
- `order.created` - Qua Outbox → RabbitMQ → User & Report Service

**Event tiếp nhận:**
- `stock.updated` - Cập nhật bảng `ProductStockCaches` (không quản lý kho chính)

## Testing

Chạy unit tests:
```bash
dotnet test
```

## Tác giả

Nhóm 2 - Order & Sales Service

## Giấy phép

MIT
