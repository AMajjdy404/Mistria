using System;

namespace GateOfEgypt.Domain.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Controller { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
