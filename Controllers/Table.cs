using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPos.Dtos;
using MyPos.Models;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace MyPos.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TableController : ControllerBase
    {
        [HttpPost("AddTable")]
        public IActionResult AddTable([FromBody] TableDTO tableDto)
        {
            if (!AuthHelper.IsAdmin(User))
            {
                return Forbid("You are not authorized to do the action");
            }

            var tableName = $"Bàn {tableDto.Number}";

            using (var context = new MyDbContext())
            {
                var existingTable = context.Table
                    .FirstOrDefault(t => t.Name.ToLower() == tableName.ToLower());

                if (existingTable != null)
                {
                    return BadRequest("Table already exists.");
                }

                var newTable = new Table
                {
                    Name = tableName
                };

                context.Table.Add(newTable);
                context.SaveChanges();

                // Generate QR code after saving to get newTable.Id
                var qrService = new QRService();
                var frontendUrl = $"https://your-frontend-url.com/order?tableId={newTable.Id}";
                var qrCodeBase64 = qrService.GenerateQRCodeBase64(frontendUrl);

                return Ok(new
                {
                    message = "Table added successfully.",
                    qrCode = qrCodeBase64
                });
            }
        }
    }

    public class QRService
    {
        public string GenerateQRCodeBase64(string url)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCode(qrData);
            using var bitmap = qrCode.GetGraphic(20);
            using var ms = new MemoryStream();

            bitmap.Save(ms, ImageFormat.Png);
            var base64 = Convert.ToBase64String(ms.ToArray());
            return $"data:image/png;base64,{base64}";
        }
    }
}
