using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rpg.Models
{
    public class CharacterSkill
    {
        public int CharacterId { get; set; }
        public Character character { get; set; }  
        public int SkillId { get; set; } 
        public Skill skill { get; set; }
    }
}