using Domain.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ChatSession: BaseEntity
    {
        public required string Title {  get; set; }
        public Guid UserId { get; set; }
        public Guid? PrescriptionId { get; set; }
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public Prescription? Prescription { get; set; }
        public User? User { get; set; }

    }
}
