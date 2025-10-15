using Markdig;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseApp.Controllers;

[ApiController]
[Route("project-info")]
public class ProjectInfoController : ControllerBase
{
  [HttpGet]
  public ContentResult Get()
  {
    // Find README.md by walking up from the app base directory until found
    string? readmePath = null;
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
      var candidate = Path.Combine(dir.FullName, "README.md");
      if (System.IO.File.Exists(candidate))
      {
        readmePath = candidate;
        break;
      }
      dir = dir.Parent;
    }

    if (string.IsNullOrEmpty(readmePath))
    {
      // Fallback simple HTML if README.md not found
      var html = @"<!doctype html><html><head><meta charset='utf-8'><title>Project Info</title></head><body style='font-family:Segoe UI, Roboto, Arial; padding:24px;'><h1>EnterpriseApp Project</h1><p>README.md not found in repository root.</p><p>APIs: <a href='/swagger'>Swagger UI</a></p></body></html>";
      return new ContentResult { Content = html, ContentType = "text/html" };
    }

    // Render README.md as HTML
    var md = System.IO.File.ReadAllText(readmePath);
    var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    var htmlBody = Markdown.ToHtml(md, pipeline);

    // Wrap the rendered HTML in a full HTML document with styling
    var fullHtml = $"<!doctype html><html><head><meta charset='utf-8'><title>README</title><link rel=\"stylesheet\" href=\"/css/site.css\"></head><body><main class=\"container\">{htmlBody}<footer><small>Rendered README.md</small></footer></main></body></html>";
    return new ContentResult { Content = fullHtml, ContentType = "text/html" };
  }
}
