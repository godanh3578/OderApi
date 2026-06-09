# Bao cao tien do Nhom 2 - Order & Sales Service

Ngay cap nhat: 09/06/2026

## 1. Tong quan

Nhom 2 da xay dung service Order & Sales trong kien truc microservices. Service phu trach xu ly don hang, khach hang, thanh toan, cong no, checkout ban hang va dong bo ton kho tu Nhom 1.

Cong nghe chinh:

- Backend: ASP.NET Core 8 Web API
- Database: SQL Server, Entity Framework Core migration
- Messaging: RabbitMQ, Outbox pattern
- Auth: JWT Bearer theo role Admin, Sales, Warehouse
- Frontend: Vue 3 + Vite trong thu muc `frontend`

## 2. Chuc nang da hoan thanh

### Backend API

- Auth demo: dang nhap sinh JWT theo role.
- Customers: them, sua, xoa mem, xem danh sach, dang ky/demo login khach hang.
- Suppliers: CRUD nha cung cap.
- Orders: tao don, xem danh sach, xem chi tiet, tra cuu don theo ma/so dien thoai, cap nhat trang thai.
- Sales: checkout, tinh tong tien, ap dung giam gia.
- Payments: ghi nhan thanh toan.
- Debts: quan ly cong no phat sinh tu don hang tra mot phan.
- ProductStockCaches: cache san pham/ton kho nhan tu Nhom 1.
- OutboxMessages: theo doi event `order.created` de day sang service khac.

### Tich hop voi Nhom 1

- Da co model/event `StockUpdatedEvent`.
- Da co background consumer `StockConsumerService` lang nghe RabbitMQ event `stock.updated`.
- Khi nhan event tu Nhom 1, service cap nhat bang `ProductStockCaches`.
- Frontend co man hinh/luong su dung du lieu ton kho cache de checkout.

### Tich hop event di

- Khi checkout tao don thanh cong, service tao outbox message `order.created`.
- `OutboxDispatcherService` doc outbox va publish qua RabbitMQ.
- Neu RabbitMQ chua chay, message giu trang thai/retry theo outbox.

### Frontend

- Da co ung dung Vue chinh trong `frontend`.
- Vite proxy `/api` ve backend `http://localhost:5002`.
- Co luong demo khach hang/sales, gio hang, checkout, quan ly don va cac man hinh lien quan.

### Docker va cau hinh

- Co `docker-compose.yml` gom SQL Server, RabbitMQ va API.
- Co `OrderApi/appsettings.Docker.json`.
- Co `OrderApi/Dockerfile`.

### Test

- Co project `OrderApi.Tests`.
- Da co test cho Customers, Orders, DebtService.

## 3. Endpoint quan trong de Nhom 1 kiem tra

Backend mac dinh:

- API: `http://localhost:5002`
- Swagger: `http://localhost:5002/swagger`

Endpoint lien quan den dong bo ton kho:

- `GET /api/ProductStockCaches`

Event Nhom 1 can publish:

- Queue/routing name dang dung: `stock.updated`
- Payload tham chieu: xem `OrderApi/Events/StockUpdatedEvent.cs`

Event Nhom 2 publish sau khi tao don:

- Event: `order.created`
- Payload tham chieu: xem `OrderApi/Events/OrderCreatedEvent.cs`

## 4. Cach chay nhanh

Backend:

```bash
cd OrderApi
dotnet run --launch-profile http
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Docker:

```bash
docker compose up --build
```

## 5. Trang thai kiem tra gan nhat

- Backend build thanh cong voi `dotnet build OrderApi/OrderApi.csproj`.
- Backend da mo duoc Swagger tai `http://localhost:5002/swagger`.
- Frontend Vite da mo duoc tai `http://localhost:5173`.
- SQL Server local `Server=.` va RabbitMQ local chua san sang trong lan kiem tra gan nhat, nen cac API can du lieu that se can bat SQL/RabbitMQ hoac chay Docker Compose.

## 6. Tep nen xem trong goi zip

- `README.md`: huong dan tong quan va API chinh.
- `docker-compose.yml`: cau hinh SQL Server, RabbitMQ va API.
- `OrderApi/Program.cs`: dang ky service, auth, CORS, migration, background services.
- `OrderApi/Events/StockUpdatedEvent.cs`: contract nhan tu Nhom 1.
- `OrderApi/Services/StockConsumerService.cs`: consumer nhan `stock.updated`.
- `OrderApi/Models/ProductStockCache.cs`: bang cache ton kho.
- `OrderApi/Controllers/StockCacheController.cs`: API xem cache ton kho.
- `OrderApi/Events/OrderCreatedEvent.cs`: event tao don.
- `OrderApi/Services/OutboxDispatcherService.cs`: publish outbox sang RabbitMQ.
