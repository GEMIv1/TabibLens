using Domain.Entities.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class User: BaseEntity
    {
        public required string UserName {  get; set; }
        public required string Email {  get; set; }
        public required string HashedPassword { get; set; }
        public string? PhoneNumber {  get; set; }
        public bool IsActive {  get; set; }
        public DateTimeOffset? EmailConfirmedAt {  get; set; }
        public DateTimeOffset? LastLoginAt { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
    }
}
