using EmployeeAPI.Data.Repositories;
using EmployeeAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAPI.Controllers
{
    [ApiController]
    [Route("/weather")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeRepository employeeRepository)
        {
            _logger = logger;
            _employeeRepository = employeeRepository;
        }


        [HttpGet]
        [Route("/employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            return Ok(await _employeeRepository.GetAllEmployees());
        }
        [HttpGet]
        [Route("/employees/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            return Ok(await _employeeRepository.GetEmployeeById(id));
        }

        [HttpPost]
        [Route("/employees")]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _employeeRepository.CreateEmployee(employee);
            return Created("Created", result);
        }
        [HttpPut]
        [Route("/employees/{id}")]
        public async Task<IActionResult> UpdateEmployee([FromBody] Employee employee)
        {
            if (employee == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _employeeRepository.UpdateEmployee(employee);
            return NoContent();
        }
        [HttpDelete]
        [Route("/employees/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            await _employeeRepository.DeleteEmployee(id);

            return NoContent();
        }

    }

}