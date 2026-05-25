using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderApi.Controllers;
using OrderApi.Data;
using OrderApi.Models;
using Xunit;

namespace OrderApi.Tests.Tests
{
    public class OrdersControllerTests
    {
        [Fact]
        public async Task Create_Get_Update_Delete_Order()
        {
            var options = new DbContextOptionsBuilder<OrderDbContext>()
                .UseInMemoryDatabase(databaseName: "OrdersTestDb_" + Guid.NewGuid())
                .Options;

            using var context = new OrderDbContext(options);
            var controller = new OrdersController(context);

            var order = new Order
            {
                CustomerName = "John",
                TotalAmount = 100,
                Items = new List<OrderItem> { new OrderItem { ProductName = "P1", Price = 50, Quantity = 2 } }
            };

            // Create
            var createActionResult = await controller.Create(order);
            Order created = null;
            if (createActionResult is OkObjectResult createOk)
            {
                created = Assert.IsType<Order>(createOk.Value);
                Assert.Equal("John", created.CustomerName);
            }
            else if (createActionResult is ObjectResult createObj)
            {
                throw new Xunit.Sdk.XunitException($"Create returned status {createObj.StatusCode}: {createObj.Value}");
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"Create returned unexpected result: {createActionResult.GetType().Name}");
            }

            // GetAll
            var getAllAction = await controller.GetAll();
            if (getAllAction is OkObjectResult getAllOk)
            {
                var list = Assert.IsType<List<Order>>(getAllOk.Value);
                Assert.Single(list);
            }
            else if (getAllAction is ObjectResult getAllObj)
            {
                throw new Xunit.Sdk.XunitException($"GetAll returned status {getAllObj.StatusCode}: {getAllObj.Value}");
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"GetAll returned unexpected result: {getAllAction.GetType().Name}");
            }

            var id = created!.Id;

            // GetById
            var getByIdAction = await controller.GetById(id);
            Order fetched = null;
            if (getByIdAction is OkObjectResult getByIdOk)
            {
                fetched = Assert.IsType<Order>(getByIdOk.Value);
                Assert.Equal(id, fetched.Id);
            }
            else if (getByIdAction is ObjectResult getByIdObj)
            {
                throw new Xunit.Sdk.XunitException($"GetById returned status {getByIdObj.StatusCode}: {getByIdObj.Value}");
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"GetById returned unexpected result: {getByIdAction.GetType().Name}");
            }
            Assert.Equal(id, fetched.Id);

            // Update
            var updatedOrder = new Order { CustomerName = "Jane", TotalAmount = 200, Items = fetched!.Items };
            var updateAction = await controller.Update(id, updatedOrder);
            if (updateAction is OkObjectResult updateOk)
            {
                var updated = Assert.IsType<Order>(updateOk.Value);
                Assert.Equal("Jane", updated.CustomerName);
            }
            else if (updateAction is ObjectResult updateObj)
            {
                throw new Xunit.Sdk.XunitException($"Update returned status {updateObj.StatusCode}: {updateObj.Value}");
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"Update returned unexpected result: {updateAction.GetType().Name}");
            }

            // Delete
            var deleteAction = await controller.Delete(id);
            if (deleteAction is OkResult || deleteAction is OkObjectResult)
            {
                // ok
            }
            else if (deleteAction is ObjectResult deleteObj)
            {
                throw new Xunit.Sdk.XunitException($"Delete returned status {deleteObj.StatusCode}: {deleteObj.Value}");
            }
            else
            {
                throw new Xunit.Sdk.XunitException($"Delete returned unexpected result: {deleteAction.GetType().Name}");
            }

            // Verify deletion
            var afterDelete = await controller.GetById(id);
            Assert.IsType<NotFoundResult>(afterDelete);
        }
    }
}
