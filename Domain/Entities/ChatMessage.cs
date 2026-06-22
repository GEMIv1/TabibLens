using Domain.Entities.Abstractions;
using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ChatMessage: BaseEntity
    {
        public Guid ChatSessionId { get; set; }
        public required string Content {  get; set; }
        public MessageRole Role { get; set; }
        public ChatSession? ChatSession { get; set; }
    }
}
