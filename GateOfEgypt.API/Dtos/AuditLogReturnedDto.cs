using System;

namespace GateOfEgypt.API.Dtos
{
    public class AuditLogReturnedDto
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public string HttpMethod { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string? EntityId { get; set; }
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
