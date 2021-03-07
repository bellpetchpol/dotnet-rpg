using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet_rpg.Data;
using dotnet_rpg.Dtos.Fight;
using dotnet_rpg.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rpg.Services.FightService
{
    public class FightService : IFightService
    {
        private readonly DataContext _context;
        public FightService(DataContext context)
        {
            _context = context;

        }

        public async Task<ServiceResponse<FightResultDto>> Fight(FightRequestDto newFightRequest)
        {
            ServiceResponse<FightResultDto> response = new ServiceResponse<FightResultDto>{
                Data = new FightResultDto()
            };
            try
            {
                List<Character> characters = await _context.Characters
                    .Include(c => c.Weapon)
                    .Include(c => c.CharacterSkills).ThenInclude(cs => cs.skill)
                    .Where(c => newFightRequest.CharacterIds.Contains(c.Id)).ToListAsync();
                bool defeated = false;
                while (!defeated)
                {
                    foreach (Character attacker in characters)
                    {
                        List<Character> opponents = characters.Where(c => c.Id != attacker.Id).ToList();
                        Character opponent = opponents[new Random().Next(opponents.Count())];

                        int damage = 0;
                        string attackUsed = String.Empty;
                        bool isUseWeapon = new Random().Next(2) == 0;
                        if(isUseWeapon)
                        {
                            attackUsed = attacker.Weapon.Name;
                            damage = DoWeaponAttack(attacker, opponent);
                        }
                        else 
                        {
                            CharacterSkill characterSkill = attacker.CharacterSkills[new Random().Next(attacker.CharacterSkills.Count())];
                            attackUsed = characterSkill.skill.Name;
                            damage = DoSkillAttack(attacker, opponent, characterSkill);
                        }
                        if (damage > 0)
                        {
                            opponent.HitPoints -= damage;
                        }
                        response.Data.Log.Add($"{attacker.Name} attack {opponent.Name} using {attackUsed} by {(damage >= 0 ? damage : 0)} damage.");

                        if(opponent.HitPoints <= 0)
                        {
                            defeated = true;
                            attacker.Victories ++;
                            opponent.Defeats ++;
                            response.Data.Log.Add($"{opponent.Name} has been defeated!");
                            response.Data.Log.Add($"{attacker.Name} won with {attacker.HitPoints} HP Left!");
                            break;
                        }
                    }
                }
                characters.ForEach(c => {
                    c.Fights++;
                    c.HitPoints = 100;
                });
                _context.Characters.UpdateRange(characters);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        public async Task<ServiceResponse<AttackResultDto>> SkillAttack(SkillAttackDto newSkillAttack)
        {
            ServiceResponse<AttackResultDto> response = new ServiceResponse<AttackResultDto>();
            try
            {
                Character attacker = await _context.Characters
                    .Include(c => c.CharacterSkills).ThenInclude(cs => cs.skill)
                    .FirstOrDefaultAsync(c => c.Id == newSkillAttack.AttackerId);
                Character opponent = await _context.Characters
                    .FirstOrDefaultAsync(c => c.Id == newSkillAttack.OpponentId);

                CharacterSkill characterSkill = attacker.CharacterSkills.FirstOrDefault(cs => cs.SkillId == newSkillAttack.SkillId);
                if (characterSkill == null)
                {
                    response.Success = false;
                    response.Message = $"{attacker.Name} doesn't has that skill.";
                    return response;
                }
                int damage = DoSkillAttack(attacker, opponent, characterSkill);
                if (damage > 0)
                {
                    opponent.HitPoints -= damage;
                }
                if (opponent.HitPoints <= 0)
                {
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                _context.Characters.Update(opponent);
                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        private static int DoSkillAttack(Character attacker, Character opponent, CharacterSkill characterSkill)
        {
            int damage = characterSkill.skill.Damage + (new Random().Next(attacker.Intelligence));
            damage -= (new Random().Next(opponent.Defend));
            return damage;
        }

        public async Task<ServiceResponse<AttackResultDto>> WeaponAttack(WeaponAttackDto newWeaponAttack)
        {
            ServiceResponse<AttackResultDto> response = new ServiceResponse<AttackResultDto>();
            try
            {
                Character attacker = await _context.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == newWeaponAttack.AttackerId);
                Character opponent = await _context.Characters
                    .Include(c => c.Weapon)
                    .FirstOrDefaultAsync(c => c.Id == newWeaponAttack.OpponentId);
                int damage = DoWeaponAttack(attacker, opponent);
                if (damage > 0)
                {
                    opponent.HitPoints -= damage;
                }
                if (opponent.HitPoints <= 0)
                {
                    response.Message = $"{opponent.Name} has been defeated!";
                }

                _context.Characters.Update(opponent);
                await _context.SaveChangesAsync();

                response.Data = new AttackResultDto
                {
                    Attacker = attacker.Name,
                    Opponent = opponent.Name,
                    AttackerHP = attacker.HitPoints,
                    OpponentHP = opponent.HitPoints,
                    Damage = damage
                };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                return response;
            }
            return response;
        }

        private static int DoWeaponAttack(Character attacker, Character opponent)
        {
            int damage = attacker.Weapon.Damage + (new Random().Next(attacker.Strength));
            damage -= (new Random().Next(opponent.Defend));
            return damage;
        }
    }
}