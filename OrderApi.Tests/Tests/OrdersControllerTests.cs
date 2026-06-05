using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private static (OrderDbContext context, OrdersController controller) CreateController()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase("OrdersTestDb_" + Guid.NewGuid())
                .Options;

            var context = new OrderDbContext(options);
            context.Customers.Add(new Customer
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

            var getAll = await controller.GetAll(null);
            var list = Assert.IsType<List<OrderDto>>((getAll as OkObjectResult)!.Value);
            Assert.Single(list);

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
    }
}
