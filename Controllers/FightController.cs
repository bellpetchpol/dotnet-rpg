using System.Threading.Tasks;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Services.FightService;
using Microsoft.AspNetCore.Mvc;

namespace dotnet_rpg.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FightController : ControllerBase
    {
        private readonly IFightService _fightService;
        public FightController(IFightService fightService)
        {
            _fightService = fightService;

        }

        [HttpPost("weapon")]
        public async Task<ActionResult> WeaponAttack(WeaponAttackDto newWeaponAttack)
        {
            return Ok(await _fightService.WeaponAttack(newWeaponAttack));
        }

        [HttpPost("skill")]
        public async Task<ActionResult> SkillAttack(SkillAttackDto newSkillAttack)
        {
            return Ok(await _fightService.SkillAttack(newSkillAttack));
        }
        [HttpPost]
        public async Task<ActionResult> Fight(FightRequestDto newFightRequest)
        {
            return Ok(await _fightService.Fight(newFightRequest));
        }
    }
}