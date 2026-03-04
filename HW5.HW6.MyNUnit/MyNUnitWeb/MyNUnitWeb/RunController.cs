// <copyright file="RunController.cs" author="bogdanovaarina">
// under MIT License
// </copyright>

using Microsoft.AspNetCore.Mvc;

namespace DefaultNamespace;

[ApiController]
[Route("api/[controller]")]
public class RunController : ControllerBase
{
    private readonly TestRunService _testRunService;

    public RunController(TestRunService testRunService)
    {
        _testRunService = testRunService;
    }

    [HttpGet("{id}")]
    public IActionResult GetRun(Guid id)
    {
        var run = _testRunService.GetRun(id);
        if (run == null)
            return NotFound();

        return Ok(run);
    }

    [HttpGet]
    public IActionResult GetAllRuns()
    {
        var runs = _testRunService.GetAllRuns();
        return Ok(runs);
    }
}