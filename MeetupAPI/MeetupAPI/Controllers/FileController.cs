using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MeetupAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        [HttpGet]
        public ActionResult GetFile(string name)
        {
            var rootFolder = Directory.GetCurrentDirectory();
            // var fileFullPath = $"{rootFolder}/PrivateAssets/{name}";
            var fileFullPath = rootFolder + "/PrivateAssets/" + name;

            if (!System.IO.File.Exists(fileFullPath))
            {
                return NotFound();
            }

            var file = System.IO.File.ReadAllBytes(fileFullPath);
            var fileProvider = new FileExtensionContentTypeProvider();

            fileProvider.TryGetContentType(fileFullPath, out var contentType);

            return File(file, contentType, name);
        }
    }
}
