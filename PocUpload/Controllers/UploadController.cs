using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PocUpload.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        private readonly ResponseContext _responseData;
        private readonly IManageFileRepository _manageFileRepository;
        private readonly ILogger<UploadController> _logger;

        public UploadController(ILogger<UploadController> logger,
            IManageFileRepository manageFileRepository)
        {
            _logger = logger;
            _manageFileRepository = manageFileRepository;
            _responseData = new ResponseContext();
        }

        [HttpPost("UploadChunk")]
        public async Task<IActionResult> UploadChunk(int id, string fileName)
        {
            try
            {
                await _manageFileRepository.CreateChunk(id, fileName, Request.Body);
            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }

        [HttpPost("UploadComplete")]
        public IActionResult UploadComplete(string fileName)
        {
            try
            {
                _manageFileRepository.CreateFile(fileName);
            }
            catch (Exception ex)
            {
                _responseData.ErrorMessage = ex.Message;
                _responseData.IsSuccess = false;
            }
            return Ok(_responseData);
        }
    }
}
