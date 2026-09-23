using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GridLink.Models


{
    public class Users
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public  string Id { get; set; } = string.Empty;

        [BsonElement("fullname")]
        public  string FullName { get; set; } = string.Empty;

        [BsonElement("nic")]
        public  string NIC { get; set; } = string.Empty;

        [BsonElement("email")]
        public  string Email { get; set; } = string.Empty;

        [BsonElement("phonenumber")]
        public  string PhoneNumber { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("hashpassword")]
        public  string HashPassword { get; set; } = string.Empty;

        [BsonElement("role")]
        public  string Role { get; set; } = UserRole.Prosumer;

        [BsonElement("status")]
        public  string Status { get; set; } = UserStatus.PendingActivation;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
