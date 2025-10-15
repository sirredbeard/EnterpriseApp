using Autofac;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EnterpriseApp.Controllers;

[ApiController]
[Route("diag")]
public class DiagnosticController : ControllerBase
{
    private readonly ILifetimeScope _scope;

    public DiagnosticController(ILifetimeScope scope)
    {
        // Setup Autofac lifetime scope
        _scope = scope;
    }

    [HttpGet("json")]
    public ContentResult JsonTest()
    {
        // Demonstrate JSON serialization with Newtonsoft.Json
        var obj = new { Time = DateTime.UtcNow, Message = "Hello from Newtonsoft.Json" };
        var s = JsonConvert.SerializeObject(obj, Formatting.Indented);
        return new ContentResult { Content = s, ContentType = "application/json" };
    }

    [HttpGet("resolve")]
    public IActionResult ResolveService()
    {
        // Demonstrate resolving a service from Autofac
        var svc = _scope.ResolveOptional<EnterpriseApp.Services.IProductService>();
        return svc == null ? NotFound("IProductService not registered") : Ok("Resolved IProductService") ;
    }
}
