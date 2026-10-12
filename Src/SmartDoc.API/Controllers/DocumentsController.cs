using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartDoc.API.Exceptions;
using SmartDoc.Models.APIModels.DocumentsModel;
using SmartDoc.Services.Interfaces;

namespace SmartDoc.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IDcoumentsService _documentService;
        public DocumentsController(IDcoumentsService documentService)
        {
            _documentService = documentService;
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFileAsync([FromForm] UploadDocumentRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                throw new BadRequestException("No file uploaded.");
            }

            var response = await _documentService.UploadFileAsync(request);

            return Ok(response);
        }

        [HttpGet("GetDocumentStatusAsync/{id}")]
        public async Task<IActionResult> GetDocumentStatusAsync([FromRoute] Guid id)
        {
            if(id == null)
            {
                throw new BadRequestException("Id not be Null.");
            }

            var response = await _documentService.GetDocumentStatusAsync(id);

            return Ok(response);
        }
    }
}