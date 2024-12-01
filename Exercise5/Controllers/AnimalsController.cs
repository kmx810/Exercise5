using Exercise5.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Exercise5.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly SqlConnection _sqlConnection;

    public AnimalsController(SqlConnection sqlConnection)
    {
        _sqlConnection = sqlConnection;
    }

    [HttpGet]
    public async Task<IActionResult> GetAnimals([FromQuery] string? orderBy = "name")
    {
        string[] allowedColumns = { "idanimal", "name", "description", "category", "area" };
        if (!allowedColumns.Contains(orderBy.ToLower()))
            return BadRequest("Invalid sort column.");

        string query = $"SELECT * FROM Animals ORDER BY {orderBy}";

        await _sqlConnection.OpenAsync();
        var command = new SqlCommand(query, _sqlConnection);

        var animals = new List<Animal>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            animals.Add(new Animal
            {
                IdAnimal = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                Category = reader.GetString(3),
                Area = reader.GetString(4)
            });
        }
        await _sqlConnection.CloseAsync();
        return Ok(animals);
    }

    [HttpPost]
    public async Task<IActionResult> AddAnimal([FromBody] Animal animal)
    {
        string query = @"
        INSERT INTO Animals (Name, Description, Category, Area) 
        OUTPUT INSERTED.IdAnimal 
        VALUES (@Name, @Description, @Category, @Area)";

        await _sqlConnection.OpenAsync();

        var command = new SqlCommand(query, _sqlConnection);
        command.Parameters.AddWithValue("@Name", animal.Name);
        command.Parameters.AddWithValue("@Description", animal.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Category", animal.Category);
        command.Parameters.AddWithValue("@Area", animal.Area);

        var newId = (int)await command.ExecuteScalarAsync();

        await _sqlConnection.CloseAsync();

        animal.IdAnimal = newId;

        return CreatedAtAction(nameof(GetAnimals), new { id = newId }, animal);
    }


    [HttpPut("{idAnimal}")]
    public async Task<IActionResult> UpdateAnimal(int idAnimal, [FromBody] Animal animal)
    {
        string query = "UPDATE Animals SET Name = @Name, Description = @Description, Category = @Category, Area = @Area WHERE IdAnimal = @IdAnimal";
        await _sqlConnection.OpenAsync();
        var command = new SqlCommand(query, _sqlConnection);
        command.Parameters.AddWithValue("@IdAnimal", idAnimal);
        command.Parameters.AddWithValue("@Name", animal.Name);
        command.Parameters.AddWithValue("@Description", animal.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Category", animal.Category);
        command.Parameters.AddWithValue("@Area", animal.Area);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        await _sqlConnection.CloseAsync();

        if (rowsAffected == 0)
            return NotFound("Animal not found.");

        return NoContent();
    }

    [HttpDelete("{idAnimal}")]
    public async Task<IActionResult> DeleteAnimal(int idAnimal)
    {
        string query = "DELETE FROM Animals WHERE IdAnimal = @IdAnimal";
        await _sqlConnection.OpenAsync();
        var command = new SqlCommand(query, _sqlConnection);
        command.Parameters.AddWithValue("@IdAnimal", idAnimal);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        await _sqlConnection.CloseAsync();

        if (rowsAffected == 0)
            return NotFound("Animal not found.");

        return NoContent();
    }
}
