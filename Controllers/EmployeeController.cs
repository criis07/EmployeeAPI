using System.Globalization;
using EmployeeAPI.Data.Repositories;
using EmployeeAPI.Model;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Asn1.Ocsp;

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

        /// <summary>
        /// Retrieves all employees.
        /// </summary>
        /// <returns>Returns a list of all employees.</returns>
        [HttpGet]
        [Route("/employees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            return Ok(await _employeeRepository.GetAllEmployees());
        }

        /// <summary>
        /// Retrieves an employee by ID.
        /// </summary>
        /// <param name="id">The ID of the employee to retrieve.</param>
        /// <returns>Returns the employee with the specified ID.</returns>
        [HttpGet]
        [Route("/employees/{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            return Ok(await _employeeRepository.GetEmployeeById(id));
        }

        /// <summary>
        /// Creates a new employee.
        /// </summary>
        /// <param name="employee">The employee data.</param>
        /// <returns>Returns a response indicating whether the employee was created successfully.</returns>
        [HttpPost]
        [Route("/employees")]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
        {
            var response = new Response { };
            // Validación de campos requeridos
            var validations = new List<string>();
            if (string.IsNullOrWhiteSpace(employee.EmployeeFirstName))
            {
                validations.Add("EmployeeFirstName");
            }
            if (string.IsNullOrWhiteSpace(employee.EmployeeLastName))
            {
                validations.Add("EmployeeLastName");
            }
            if (string.IsNullOrWhiteSpace(employee.EmployeePhone))
            {
                validations.Add("EmployeePhone");
            }
            if (string.IsNullOrWhiteSpace(employee.EmployeeZip))
            {
                validations.Add("EmployeeZip");
            }

            if (validations.Any())
            {
                response = new Response
                {
                    message = $"Los campos '{string.Join(", ", validations)}' son requeridos",
                    succeeded = false
                };
                return StatusCode(400, response);
            }
            

            if (employee.EmployeePhone.Length != 10)
            {
                response = new Response
                {
                    message = "Phone number must have 10 digits",
                    succeeded = false
                };
                return StatusCode(400, response);
            }
            else
            {
                var request = employee;
                request.EmployeePhone.Trim().Replace("-", "");
                string areaCode = request.EmployeePhone.Substring(0, 3);
                string prefix = request.EmployeePhone.Substring(3, 3);
                string number = request.EmployeePhone.Substring(6, 4);

                request.EmployeePhone = $"({areaCode}) {prefix}-{number}";
                employee = request;
            }

            employee.HireDate = DateTime.ParseExact(employee.HireDate.ToString("MM/dd/yyyy"), "MM/dd/yyyy", CultureInfo.InvariantCulture);

            DateTime parsedDate;
            if (DateTime.TryParseExact(employee.HireDate.ToString("MM/dd/yyyy"), "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                employee.HireDate = parsedDate;
            }
            else
            {
                response = new Response
                {
                    message = "Date sent in wrong format",
                    succeeded = false
                };
                return StatusCode(200, response);
            }


            if (employee == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createContract = new CreateEmployeeContract
            {
                EmployeeFirstName = employee.EmployeeFirstName,
                EmployeeLastName = employee.EmployeeLastName,
                EmployeePhone = employee.EmployeePhone,
                EmployeeZip = employee.EmployeeZip,
                HireDate = employee.HireDate.ToString(),
            };
            string[] splittedDate = createContract.HireDate.Split(' ');
            createContract.HireDate = splittedDate[0];

            var result = await _employeeRepository.CreateEmployee(createContract);
            if (result)
            {
               response = new Response
               {
                    message = "Created",
                    succeeded = true
               };
                return StatusCode(200, response);
            }
            response = new Response
            {
                message = "Something went wrong",
                succeeded = false
            };
            return StatusCode(400, response);
        }

        /// <summary>
        /// Updates an existing employee.
        /// </summary>
        /// <param name="employee">The updated employee data.</param>
        /// <returns>Returns a response indicating the success of the update operation.</returns>
        [HttpPut]
        [Route("/employees/{id}")]
        public async Task<IActionResult> UpdateEmployee([FromBody] Employee employee)
        {
            var response = new Response { };
            if (employee == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _employeeRepository.UpdateEmployee(employee);
            if (result) { 
            response = new Response
            {
                message = "updated",
                succeeded = true
            };
            return StatusCode(200, response);
            }
            response = new Response
            {
                message = "Something went wrong",
                succeeded = false
            };
            return StatusCode(400, response);
        }

        /// <summary>
        /// Deletes an employee by ID.
        /// </summary>
        /// <param name="id">The ID of the employee to delete.</param>
        /// <returns>Returns a response indicating the success of the delete operation.</returns>
        [HttpDelete]
        [Route("/employees/{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var response = new Response { };
            var result = await _employeeRepository.DeleteEmployee(id);
            if (result)
            {
                response = new Response
                {
                    message = "Deleted",
                    succeeded = true
                };
                return StatusCode(200, response);
            }
            response = new Response
            {
                message = "Something went wrong",
                succeeded = false
            };
            return StatusCode(400, response);
        }
        /// <summary>
        /// Searches for employees by last name or phone number and returns all 6 values.
        /// </summary>
        /// <param name="query">The last name or phone number to search for.</param>
        /// <returns>Returns a list of employees matching the search criteria.</returns>
        [HttpGet]
        [Route("/employees/search")]
        public async Task<IActionResult> SearchEmployees(string param)
        {
            var response = new Response { };
            if (string.IsNullOrWhiteSpace(param))
            {
                response = new Response
                {
                    message = "Param for this search is required",
                    succeeded = false
                };
                return StatusCode(400, response);
            }

            var employees = await _employeeRepository.SearchEmployees(param);

            if (employees == null || !employees.Any())
            {             
                response = new Response
                {
                    message = "No employees found",
                    succeeded = false
                };
                return StatusCode(400, response);
            }

            return Ok(employees);
        }

    }

}