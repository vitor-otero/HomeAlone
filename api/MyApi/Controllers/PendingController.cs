using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyApi.Models;
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
    public class PendingController : ControllerBase
    {
        private readonly string _connectionString = AppConfig.DBConnection;


        [HttpGet]
        public IActionResult Get()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            List<Pending> pendings = new List<Pending>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
                        {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id, description, due_date, is_completed FROM Pending WHERE user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pendings.Add(new Pending
                        {
                            Id = reader.GetInt32("id"),
                            Description = reader.GetString("description"),
                            DueDate = reader.GetDateTime("due_date").ToString("yyyy-MM-dd"), // Format date as string
                            IsCompleted = reader.GetBoolean("is_completed")
                        });
                    }
                }
            }

            return Ok(pendings);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Pending pending)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!DateTime.TryParseExact(pending.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
            {
                return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });
            }

            int newId;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Pending (user_id, description, due_date, is_completed) VALUES (@userId, @description, @dueDate, @isCompleted); SELECT LAST_INSERT_ID();", conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@description", pending.Description);
                cmd.Parameters.AddWithValue("@dueDate", dueDate);
                cmd.Parameters.AddWithValue("@isCompleted", pending.IsCompleted);
                newId = Convert.ToInt32(cmd.ExecuteScalar());
            }

            return Ok(new { id = newId, message = "Pending added successfully." });
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Pending pending)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            if (!DateTime.TryParseExact(pending.DueDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dueDate))
            {
                return BadRequest(new { message = "Invalid date format. Use yyyy-MM-dd." });
            }

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE Pending SET description = @description, due_date = @dueDate, is_completed = @isCompleted WHERE id = @id AND user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@description", pending.Description);
                cmd.Parameters.AddWithValue("@dueDate", dueDate);
                cmd.Parameters.AddWithValue("@isCompleted", pending.IsCompleted);
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Pending not found or not authorized to update." });
                }
            }

            return Ok(new { id = id, message = "Pending updated successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Pending WHERE id = @id AND user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId", userId);
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Pending not found or not authorized to delete." });
                }
            }

            return Ok(new { id = id, message = "Pending deleted successfully." });
        }
    }
}
