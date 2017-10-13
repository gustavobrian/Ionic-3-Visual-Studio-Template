using System;
using System.ComponentModel.DataAnnotations.Schema;
using Sammaron.Core.Interfaces;

namespace Sammaron.Core.Models
{
    public class Position : IEntity<long>
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public bool IsCurrent { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}