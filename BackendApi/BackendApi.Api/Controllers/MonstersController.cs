using BackendApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendApi.Api.Controllers
{
    [ApiController]
    [Route("api/monsters")]
    public sealed class MonstersController : ControllerBase
    {
        private readonly IMonsterService _monsterService;

        public MonstersController(IMonsterService monsterService)
        {
            _monsterService = monsterService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _monsterService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{monsterKey}")]
        public async Task<IActionResult> GetByKey(string monsterKey)
        {
            var result = await _monsterService.GetByKeyAsync(monsterKey);
            if (result is null)
                return NotFound();

            return Ok(result);
        }
    }
}