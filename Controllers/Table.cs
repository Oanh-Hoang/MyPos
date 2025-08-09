using Microsoft.AspNetCore.Mvc;
using QRCoder;
using Microsoft.AspNetCore.Mvc;
using MyPos.Dtos;
using MyPos.Models;


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
                
                var frontendUrl = $"http://localhost:3000//order?tableId={newTable.Id}";
                var qrCodeBase64 = GenerateQRCodeBase64(frontendUrl);


                return Ok(new
                {
                    message = "Table added successfully.",
                    qrCode = qrCodeBase64
                });
            }
        }


        private string GenerateQRCodeBase64(string url)
        {
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(url, QRCoder.QRCodeGenerator.ECCLevel.Q);
            var pngByteQrCode = new QRCoder.PngByteQRCode(qrData);
            byte[] qrCodeBytes = pngByteQrCode.GetGraphic(20);

            return $"data:image/png;base64,{Convert.ToBase64String(qrCodeBytes)}";
        }
    }
}
