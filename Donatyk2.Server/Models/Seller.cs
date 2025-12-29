using System;
using System.Text.RegularExpressions;
using Donatyk2.Server.Data;
using Donatyk2.Server.Dto;

namespace Donatyk2.Server.Models
{
    public class Seller
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarImageUrl { get; set; }
        public Guid UserId { get; set; }

        public Seller(Guid id, string name, string description, string email, string phoneNumber, string avatarImageUrl, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Seller name cannot be null or whitespace.", nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Seller description cannot be null or whitespace.", nameof(description));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Seller email cannot be null or whitespace.", nameof(email));

            if (userId == Guid.Empty)
                throw new ArgumentException("UserId must be a valid GUID.", nameof(userId));

            // Basic RFC-like email check (sufficient for most cases; not exhaustive)
            const string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(email, emailPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                throw new ArgumentException("Seller email is not a valid email address.", nameof(email));

            // E.164 phone number format (international). Adjust pattern if you need local formats.
            const string phonePattern = @"^\+?[1-9]\d{1,14}$";
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Seller phone number cannot be null or whitespace.", nameof(phoneNumber));
            if (!Regex.IsMatch(phoneNumber, phonePattern, RegexOptions.CultureInvariant))
                throw new ArgumentException("Seller phone number is not a valid E.164 formatted number.", nameof(phoneNumber));

            Id = id;
            Name = name;
            Description = description;
            Email = email;
            PhoneNumber = phoneNumber;
            AvatarImageUrl = avatarImageUrl;
            UserId = userId;
        }

        // New: construct Seller from SellerDto + userId
        public Seller(SellerDto dto, Guid userId)
            : this(
                  dto.Id,
                  dto.Name ?? throw new ArgumentException("Name is required in SellerDto.", nameof(dto)),
                  dto.Description ?? throw new ArgumentException("Description is required in SellerDto.", nameof(dto)),
                  dto.Email ?? throw new ArgumentException("Email is required in SellerDto.", nameof(dto)),
                  dto.PhoneNumber ?? throw new ArgumentException("PhoneNumber is required in SellerDto.", nameof(dto)),
                  dto.AvatarImageUrl ?? string.Empty,
                  userId)
        {
        }

        public Seller(SellerEntity entity)
            : this(
                  entity.Id,
                  entity.Name,
                  entity.Description,
                  entity.Email,
                  entity.PhoneNumber,
                  entity.AvatarImageUrl,
                  entity.UserId)
        {
        }
    }
}
