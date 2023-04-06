#nullable disable

namespace RCL.Core.Identity.Graph
{
    public class UserClaimsData
    {
        public string ObjectId { get; set; }
        public string GivenName { get; set; }
        public string SurName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string JobTitle { get; set; }

        #region Extensions

        public DateTime? DateOfBirth { get; set; }
        public DateTime? DateVerified { get; set; }
        public string PhotoUrl { get; set; }
        public bool? UserIsVerified { get; set; }

        #endregion
    }
}
