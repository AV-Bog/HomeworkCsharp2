// <copyright file="UploadController.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

namespace DefaultNamespace;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly TestRunService _testRunService;

    public UploadController(IWebHostEnvironment env, TestRunService testRunService)
    {
        _env = env;
        _testRunService = testRunService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFileCollection files)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        var tempDir = Path.Combine(_env.ContentRootPath, "uploads", Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var paths = new List<string>();

        foreach (var file in files)
        {
            if (file.Length > 0)
            {
                var filePath = Path.Combine(tempDir, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                paths.Add(filePath);
            }
        }

        var runId = _testRunService.StartRun(paths);

        return Ok(new { runId });
    }
}