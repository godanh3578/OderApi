using Microsoft.AspNetCore.Mvc;
using OrderApi.Models;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private static List<Supplier> suppliers = new List<Supplier>();

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(suppliers);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var supplier = suppliers.FirstOrDefault(x => x.Id == id);

            if (supplier == null)
                return NotFound();

            return Ok(supplier);
        }

        [HttpPost]
        public IActionResult Create(Supplier supplier)
        {
            suppliers.Add(supplier);
            return Ok(supplier);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Supplier updatedSupplier)
        {
            var supplier = suppliers.FirstOrDefault(x => x.Id == id);

            if (supplier == null)
                return NotFound();

            supplier.Name = updatedSupplier.Name;
            supplier.Phone = updatedSupplier.Phone;

            return Ok(supplier);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var supplier = suppliers.FirstOrDefault(x => x.Id == id);

            if (supplier == null)
                return NotFound();

            suppliers.Remove(supplier);
            return Ok();
        }
    }
}