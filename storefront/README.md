# ⚠️ DEPRECATED — Không sử dụng cho nộp bài

Thư mục `storefront/` là **bản frontend cũ**, không còn được bảo trì.

## Vấn đề

- API hardcode sai host (`160.250.132.117:3000`)
- Gọi `POST /api/Orders` thay vì `POST /api/Sales/Checkout`
- Không tích hợp JWT nhân viên, không đúng luồng Nhóm 2

## Frontend chính thức

Sử dụng thư mục **`frontend/`** ở root repo:

```bash
cd frontend
npm install
npm run dev
```

API backend: `http://localhost:5002` (xem `frontend/.env.development`).
