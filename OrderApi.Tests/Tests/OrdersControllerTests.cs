using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using OrderApi.Controllers;
using OrderApi.Data;
using OrderApi.DTOs.Orders;
using OrderApi.Models;
using OrderApi.Services;
using Xunit;

namespace OrderApi.Tests.Tests
{
    public class OrdersControllerTests
    {
        private static void SetStaffUser(OrdersController controller, string role = "Sales", string username = "sales01")
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, username),
                new("role", role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth", ClaimTypes.Name, "role");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        private static (OrderDbContext context, OrdersController controller) CreateController()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase("OrdersTestDb_" + Guid.NewGuid())
                .Options;

            var context = new OrderDbContext(options);
            context.Customers.Add(new Customers
            {
                CustomerId = 1,
                CustomerCode = "KH001",
                FullName = "John",
                Phone = "0123456789",
                Email = "john@example.com",
                Address = "Somewhere"
            });
            context.ProductStockCaches.Add(new ProductStockCache
            {
                ProductId = 1,
                ProductCode = "SP001",
                ProductName = "Product 1",
                SellingPrice = 50,
                QuantityAvailable = 100,
                StockStatus = StockStatus.InStock
            });
            context.SaveChanges();

            var outbox = new OutboxService(context);
            var orderService = new OrderService(context, outbox, NullLogger<OrderService>.Instance);
            var controller = new OrdersController(orderService);
            return (context, controller);
        }

        [Fact]
        public async Task Create_Get_Update_Cancel_Delete_Order()
        {
            var (_, controller) = CreateController();
            SetStaffUser(controller);

            var dto = new CreateOrderDto
            {
                CustomerId = 1,
                CreatedBy = "sales01",
                DiscountAmount = 0,
                Items = new List<CreateOrderDetailDto>
                {
                    new() { ProductId = 1, ProductCode = "SP001", ProductName = "P1", Quantity = 2, UnitPrice = 50 }
                }
            };

            var createResult = await controller.Create(dto);
            var created = Assert.IsType<OrderDto>((createResult as OkObjectResult)!.Value);
            Assert.Equal(100, created.TotalAmount);

            var getByCustomer = await controller.GetAll(null, 1);
            var customerList = Assert.IsType<List<OrderDto>>((getByCustomer as OkObjectResult)!.Value);
            Assert.Single(customerList);

            var getAllStaff = await controller.GetAll(null, null);
            var staffList = Assert.IsType<List<OrderDto>>((getAllStaff as OkObjectResult)!.Value);
            Assert.Single(staffList);

            var getById = await controller.GetById(created.OrderId);
            var fetched = Assert.IsType<OrderDto>((getById as OkObjectResult)!.Value);
            Assert.Equal(created.OrderId, fetched.OrderId);

            var statusResult = await controller.UpdateStatus(created.OrderId, new UpdateOrderStatusRequest { Status = "Confirmed" });
            var updated = Assert.IsType<OrderDto>((statusResult as OkObjectResult)!.Value);
            Assert.Equal("Confirmed", updated.OrderStatus);

            var cancelResult = await controller.Cancel(created.OrderId);
            Assert.IsType<OkObjectResult>(cancelResult);

            var deleteResult = await controller.Delete(created.OrderId);
            Assert.IsType<OkResult>(deleteResult);

            var afterDelete = await controller.GetById(created.OrderId);
            Assert.IsType<NotFoundResult>(afterDelete);
        }

        [Fact]
        public async Task GetAll_WithoutAuth_ReturnsUnauthorized_WhenNoCustomerId()
        {
            var (_, controller) = CreateController();

            var result = await controller.GetAll(null, null);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
