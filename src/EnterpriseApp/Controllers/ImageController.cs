using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Imaging;

namespace EnterpriseApp.Controllers;

[ApiController]
[Route("image")]
public class ImageController : ControllerBase
{
    [HttpGet("thumb")]
    public IActionResult Thumb()
    {
        // Create a 128x64 thumbnail image, save as PNG
        using var bmp = new Bitmap(128, 64);
        using var g = Graphics.FromImage(bmp);
        g.Clear(Color.CornflowerBlue);
        using var pen = new Pen(Color.White, 2);
        g.DrawEllipse(pen, 10, 10, 48, 48);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        ms.Position = 0;
        return File(ms.ToArray(), "image/png");
    }
}
