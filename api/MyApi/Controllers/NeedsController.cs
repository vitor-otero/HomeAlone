using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Security.Claims;

namespace MyApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NeedsController : ControllerBase
    {
        private readonly string _connectionString = "Server=db;Database=mydatabase;User=myuser;Password=mypassword;";

        [HttpGet]
        public IActionResult Get()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            List<Need> needs = new List<Need>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT id, description, is_met, percentage_met FROM Needs WHERE user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        needs.Add(new Need
                        {
                            Id = reader.GetInt32("id"),
                            Description = reader.GetString("description"),
                            IsMet = reader.GetBoolean("is_met"),
                            PercentageMet = reader.GetInt32("percentage_met")
                        });
                    }
                }
            }

            return Ok(needs);
        }

        [HttpPost]
        public IActionResult Post([FromBody] Need need)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            int newId;

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("INSERT INTO Needs (user_id, description, is_met, percentage_met) VALUES (@userId, @description, @isMet, @percentageMet); SELECT LAST_INSERT_ID();", conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@description", need.Description);
                cmd.Parameters.AddWithValue("@isMet", need.IsMet);
                cmd.Parameters.AddWithValue("@percentageMet", need.IsMet ? 100 : need.PercentageMet);
                newId = System.Convert.ToInt32(cmd.ExecuteScalar());
            }

            return Ok(new { id = newId, message = "Need added successfully." });
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Need need)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("UPDATE Needs SET description = @description, is_met = @isMet, percentage_met = @percentageMet WHERE id = @id AND user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@description", need.Description);
                cmd.Parameters.AddWithValue("@isMet", need.IsMet);
                cmd.Parameters.AddWithValue("@percentageMet", need.IsMet ? 100 : need.PercentageMet);
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Need not found or not authorized to update." });
                }
            }

            return Ok(new { id = id, message = "Need updated successfully." });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("DELETE FROM Needs WHERE id = @id AND user_id = @userId", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@userId", userId);
                var rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected == 0)
                {
                    return NotFound(new { message = "Need not found or not authorized to delete." });
                }
            }

            return Ok(new { id = id, message = "Need deleted successfully." });
        }
    }
}
