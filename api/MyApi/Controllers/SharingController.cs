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
    public class SharingController : ControllerBase
    {
        private readonly string _connectionString = "Server=db;Database=mydatabase;User=myuser;Password=mypassword;";

        [HttpPost("share-all")]
        public IActionResult ShareAllData([FromBody] ShareAllRequest shareAllRequest)
        {
            var ownerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // Share Needs
                MySqlCommand cmd = new MySqlCommand("INSERT INTO SharedData (owner_id, shared_with_id, module, item_id) SELECT @ownerId, @sharedWithId, 'Needs', id FROM Needs WHERE user_id = @ownerId", conn);
                cmd.Parameters.AddWithValue("@ownerId", ownerId);
                cmd.Parameters.AddWithValue("@sharedWithId", shareAllRequest.SharedWithId);
                cmd.ExecuteNonQuery();

                // Share Payments
                cmd.CommandText = "INSERT INTO SharedData (owner_id, shared_with_id, module, item_id) SELECT @ownerId, @sharedWithId, 'Payments', id FROM Payments WHERE user_id = @ownerId";
                cmd.ExecuteNonQuery();

                // Share Pending
                cmd.CommandText = "INSERT INTO SharedData (owner_id, shared_with_id, module, item_id) SELECT @ownerId, @sharedWithId, 'Pending', id FROM Pending WHERE user_id = @ownerId";
                cmd.ExecuteNonQuery();
            }

            return Ok(new { message = "All data shared successfully." });
        }

        [HttpGet("shared-with-me")]
        public IActionResult GetSharedData()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var sharedData = new List<SharedData>();

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM SharedData WHERE shared_with_id = @userId", conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sharedData.Add(new SharedData
                        {
                            Id = reader.GetInt32("id"),
                            OwnerId = reader.GetInt32("owner_id"),
                            SharedWithId = reader.GetInt32("shared_with_id"),
                            Module = reader.GetString("module"),
                            ItemId = reader.GetInt32("item_id")
                        });
                    }
                }
            }

            return Ok(sharedData);
        }
    }

    public class ShareAllRequest
    {
        public int SharedWithId { get; set; }
    }

    public class SharedData
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public int SharedWithId { get; set; }
        public string Module { get; set; }
        public int ItemId { get; set; }
    }
}
