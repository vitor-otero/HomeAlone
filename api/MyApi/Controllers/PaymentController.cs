using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;

namespace MyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly string _connectionString = "Server=db;Database=mydatabase;User=myuser;Password=mypassword;";

        [HttpGet]
        public IActionResult Get()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            List<Payment> payments = new List<Payment>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id, description, amount, due_date, is_paid FROM Payments WHERE user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            Id = reader.GetInt32("id"),
                            Description = reader.GetString("description"),
                            Amount = reader.GetDecimal("amount"),
                            DueDate = reader.GetDateTime("due_date").ToString("yyyy-MM-dd"), // Format date as string
                            IsPaid = reader.GetBoolean("is_paid")
                        });
                    }
                }
            }

            return Ok(payments);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Payment payment)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!DateTime.TryParseExact(payment.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
            {
                return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });
            }

            int newId;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Payments (user_id, description, amount, due_date, is_paid) VALUES (@userId, @description, @amount, @dueDate, @isPaid); SELECT LAST_INSERT_ID();", conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@description", payment.Description);
                cmd.Parameters.AddWithValue("@amount", payment.Amount);
                cmd.Parameters.AddWithValue("@dueDate", dueDate.ToString("yyyy-MM-dd")); // Store as date
                cmd.Parameters.AddWithValue("@isPaid", payment.IsPaid);
                newId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return Ok(new { id = newId, message = "Payment added successfully." });
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Payment payment)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!DateTime.TryParseExact(payment.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
            {
                return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });
            }

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE Payments SET description = @description, amount = @amount, due_date = @dueDate, is_paid = @isPaid WHERE id = @id AND user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@description", payment.Description);
                cmd.Parameters.AddWithValue("@amount", payment.Amount);
                cmd.Parameters.AddWithValue("@dueDate", dueDate.ToString("yyyy-MM-dd")); // Store as date
                cmd.Parameters.AddWithValue("@isPaid", payment.IsPaid);
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Payment not found or not authorized to update." });
                }
            }

            return Ok(new { id = id, message = "Payment updated successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Payments WHERE id = @id AND user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId", userId);
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Payment not found or not authorized to delete." });
                }
            }

            return Ok(new { id = id, message = "Payment deleted successfully." });
        }
    }
}
