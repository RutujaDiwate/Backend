using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace backend.Models.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.UserId) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Invalid request");
            }

            // Connect to your SQL Server database
            string connectionString = "server=103.190.54.22,1633\\SQLEXPRESS;database=hrms_app;user=ecohrms;Password=EcoHrms@123;Encrypt=False";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if the user exists in the database
                string query = "SELECT TOP 1 userid, email, city, status FROM ecohrms.userdata WHERE userid = @UserId AND password = @Password";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", loginRequest.UserId);
                    command.Parameters.AddWithValue("@Password", loginRequest.Password);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // User is authenticated
                            var userData = new UserData
                            {
                                UserId = reader["userid"].ToString(),
                                Email = reader["email"].ToString(),
                                City = reader["city"].ToString(),
                                Status = "A"
                            };



                            string updateStatusQuery = "UPDATE ecohrms.userdata SET status = @Status WHERE userid = @UserId";
                            using (SqlCommand updateCommand = new SqlCommand(updateStatusQuery, connection))
                            // Begin a transaction
                            using (SqlTransaction transaction = connection.BeginTransaction())
                            {
                                updateCommand.Transaction = transaction;

                                try
                                {
                                updateCommand.Parameters.AddWithValue("@UserId", loginRequest.UserId);
                                updateCommand.Parameters.AddWithValue("@Status", "A"); // Set the status to 'A' for active state
                                updateCommand.ExecuteNonQuery();

                                    transaction.Commit();

                                }
                                catch (Exception)
                                {
                                    // Rollback the transaction if an exception occurs
                                    transaction.Rollback();
                                    throw; // Rethrow the exception
                                }
                            }
                            var response = new LoginResponse
                            {
                                IsSuccessful = true,
                                Message = "Login successful",
                                UserData = userData
                            };

                            return Ok(response);
                        }
                        else
                        {
                            // Authentication failed
                            var response = new LoginResponse
                            {
                                IsSuccessful = false,
                                Message = "Invalid credentials",
                                UserData = null
                            };

                            return Unauthorized(response);
                        }
                    }
                }
            }
        }
    }

    public class LoginRequest
    {
        public string? UserId { get; set; }
        public string? Password { get; set; }
        public string? Registrationkey { get; set; }
    }

    public class UserData
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? City { get; set; }
        public string? Status { get; set; }

        public UserData()
        {
            UserId = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }
    }

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("AllowAll");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
