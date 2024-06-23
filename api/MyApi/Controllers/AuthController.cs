using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net; // Add this namespace

namespace MyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _connectionString = "Server=db;Database=mydatabase;User=myuser;Password=mypassword;";
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Users (username, password, role) VALUES (@username, @password, 'user')", conn);
                cmd.Parameters.AddWithValue("@username", user.Username);
                cmd.Parameters.AddWithValue("@password", BCrypt.Net.BCrypt.HashPassword(user.Password)); // Updated line
                try
                {
                    cmd.ExecuteNonQuery();
                    return Ok(new { message = "User registered successfully." });
                }
                catch (MySqlException)
                {
                    return BadRequest(new { message = "Username already exists." });
                }
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] User user)
        {
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM Users WHERE username = @username", conn);
                cmd.Parameters.AddWithValue("@username", user.Username);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var storedPassword = reader.GetString("password");
                        if (BCrypt.Net.BCrypt.Verify(user.Password, storedPassword)) // Updated line
                        {
                            var token = GenerateJwtToken(reader.GetInt32("id"), reader.GetString("role"));
                            return Ok(new { token });
                        }
                    }
                }
                return Unauthorized(new { message = "Invalid username or password." });
            }
        }

        private string GenerateJwtToken(int userId, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
