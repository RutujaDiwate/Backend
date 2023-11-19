using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeAPIDbContext dbContext;

        public EmployeesController(EmployeeAPIDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult GetEmployees()
        {
            var EmployeeDetails = dbContext.EmployeeDetails.ToListAsync();
            return Ok(EmployeeDetails);
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetEmployee(string userId)
        {
            var employee = await dbContext.EmployeeDetails.FindAsync(userId);

            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] AddEmployeeRequest addEmployeeRequest)
        {
            var employee = new EmployeeDetails
            {
                userId = addEmployeeRequest.userId,
                Name = addEmployeeRequest.Name,
                Email = addEmployeeRequest.Email
            };

            await dbContext.EmployeeDetails.AddAsync(employee);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployee), new { userId = employee.userId }, employee);
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateEmployee(string userId, [FromBody] UpdateEmployeeRequest updateEmployeeRequest)
        {
            var employee = await dbContext.EmployeeDetails.FindAsync(userId);

            if (employee != null)
            {
                employee.Name = updateEmployeeRequest.Name;
                employee.Email = updateEmployeeRequest.Email;

                dbContext.EmployeeDetails.Update(employee);
                await dbContext.SaveChangesAsync();

                return Ok(employee);
            }

            return NotFound();
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteEmployee(string userId)
        {
            var employee = await dbContext.EmployeeDetails.FindAsync(userId);

            if (employee != null)
            {
                dbContext.EmployeeDetails.Remove(employee);
                await dbContext.SaveChangesAsync();

                return NoContent();
            }

            return NotFound();
        }
    }
}
