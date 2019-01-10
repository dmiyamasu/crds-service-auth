using Crossroads.Web.Common.MinistryPlatform;
using Newtonsoft.Json;

namespace MinistryPlatform.Models
{
    [MpRestApiTable(Name = "Contacts")]
    public class MpContact
    {
        [JsonProperty(PropertyName = "Contact_ID")]
        public int ContactId { get; set; }

        [JsonProperty(PropertyName = "Email_Address")]
        public string EmailAddress { get; set; }

        [JsonProperty(PropertyName = "Household_ID")]
        public int? HouseholdId { get; set; }

        [JsonProperty(PropertyName = "User_Account")]
        public int? UserAccount { get; set; }

        [JsonProperty(PropertyName = "Donor_Record")]
        public int? DonorRecord { get; set; }

        [JsonProperty(PropertyName = "Participant_Record")]
        public int? ParticipantRecord { get; set; }

        [JsonProperty(PropertyName = "Can_Impersonate")]
        public bool? CanImpersonate { get; set; }
    }
}