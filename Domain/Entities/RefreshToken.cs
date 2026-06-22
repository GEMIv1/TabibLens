using Domain.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RefreshToken: BaseEntity
    {
        public Guid UserId { get; set; }
        public required  string HashedToken { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt.HasValue;
        public bool IsActive => !IsExpired && !IsRevoked;
        public User? User { get; set; }
    }
}
