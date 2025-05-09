using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartElectricityAPI.Database;
using SmartElectricityAPI.Interfaces;

namespace SmartElectricityAPI.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserManualController : ControllerBase
{
    private readonly MySQLDBContext _dbContext;
    private readonly IUserInfo _userInfo;
    private readonly string uploadPath = Path.Combine("/app/uploads/manuals");

    public UserManualController(MySQLDBContext context, IUserInfo userInfo)
    {
        _dbContext = context;
        _userInfo = userInfo;
    }

    [HttpPost, Authorize]
    [Route("Upload")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (!_userInfo.IsAdmin)
        {
            return Unauthorized("You are not authorized to upload files.");
        }
            if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }


        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }


        var filePath = Path.Combine(uploadPath, file.FileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Optionally, save file information to the database
            // var fileRecord = new FileRecord { FileName = file.FileName, FilePath = filePath };
            // _dbContext.FileRecords.Add(fileRecord);
            // await _dbContext.SaveChangesAsync();

            return Ok(new { filePath });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("Manuals"), Authorize]
    public IActionResult GetFileNames()
    {
        try
        {

            if (!Directory.Exists(uploadPath))
            {
                return NotFound("Directory not found.");
            }

            var files = Directory.GetFiles(uploadPath)
                                 .Select(Path.GetFileName)
                                 .ToList().OrderBy(x=> x);

            return Ok(files);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("Manuals/{fileName}"), Authorize]
    public IActionResult DeleteFile(string fileName)
    {
        if (!_userInfo.IsAdmin)
        {
            return Unauthorized("You are not authorized to upload files.");
        }

        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            var filePath = Path.Combine(uploadPath, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            System.IO.File.Delete(filePath);

            return Ok("File deleted successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("files/download/{fileName}"), Authorize]
    public async Task<IActionResult> DownloadFile(string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name is required.");
            }

            var filePath = Path.Combine(uploadPath, fileName);
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("File not found.");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, GetContentType(filePath), Path.GetFileName(filePath));
        }
        catch (Exception ex)
        {
          
            return StatusCode(500, "Internal server error");
        }
    }

    private string GetContentType(string path)
    {
        var types = GetMimeTypes();
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
    }

    private Dictionary<string, string> GetMimeTypes()
    {
        return new Dictionary<string, string>
        {
            { ".txt", "text/plain" },
            { ".pdf", "application/pdf" },
            { ".doc", "application/vnd.ms-word" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".gif", "image/gif" },
            { ".csv", "text/csv" }
        };
    }
}
