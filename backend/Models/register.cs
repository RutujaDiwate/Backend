using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;

namespace backend.Models
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public RegistrationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            try
            {
                var userid = request.UserId;
                var email = request.Email;
                var password = request.Password;

                // Generate a new registration key
                var newRegistrationKey = GenerateRandomKey(10);

                // Hash the user's password
                var hashedPassword = HashPassword(password);

                // Continue with registration after hashing the password
                var smtpClient = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    Credentials = new NetworkCredential("rutuja.22110140@viit.ac.in", "zalpwsjzrxksvslk"),
                    EnableSsl = true,
                };

                // Define the email message
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("rutuja.22110140@viit.ac.in"),
                    Subject = "Registration Key",
                    Body = $"Your registration key is: {newRegistrationKey}",
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(email);

                // Send the email
                await smtpClient.SendMailAsync(mailMessage);

                // Store the new registration key and hashed password in the database
                using (var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    await connection.OpenAsync();

                    // Insert into RegistrationKeys table
                    using (var cmd = new SqlCommand("INSERT INTO ecohrms.RegistrationKeys (userid, registrationKey, status) VALUES (@userid, @registrationKey, 'R')", connection))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        cmd.Parameters.AddWithValue("@registrationKey", newRegistrationKey);
                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Insert into userdata table
                    using (var cmd = new SqlCommand("INSERT INTO ecohrms.userdata (userid, email, password, status) VALUES (@userid, @email, @password, 'R')", connection))
                    {
                        cmd.Parameters.AddWithValue("@userid", userid);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@password", hashedPassword);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                // Send the registration key as a JSON response
                return Ok(new { RegistrationKey = newRegistrationKey });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"An error occurred: {ex.Message}");
                Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private string GenerateRandomKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            throw new NotImplementedException();
        }

        private string HashPassword(string password)
        {
            // Use a secure password hashing algorithm like Argon2 or bcrypt
            var passwordHasher = new PasswordHasher<object>();
            var hashedPassword = passwordHasher.HashPassword(null, password);
            return hashedPassword;
        }

        public class RegistrationRequest
        {
            public string? UserId { get; set; }
            public string? Email { get; set; }

            // Use [JsonIgnore] attribute to ignore during serialization/deserialization
            [JsonIgnore]
            public Action? MoveNextAction { get; set; }

            public string? Password { get; set; }
        }

        // If you still want to use the custom converter, you can register it in ConfigureServices
        // services.AddControllers().AddJsonOptions(options =>
        // {
        //     options.JsonSerializerOptions.Converters.Add(new ActionConverter());
        // });
    }
}
