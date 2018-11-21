using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crossroads.Service.Auth.Models
{
    public class AuthDTO
    {
        [JsonProperty("Authentication")]
        public AuthenticationDTO Authentication { get; set; }

        [JsonProperty("Authorization")]
        public AuthorizationDTO Authorization { get; set; }

        [JsonProperty("UserInfo")]
        public UserInfoDTO UserInfo { get; set; }
    }

    public class AuthenticationDTO
    {
        [JsonProperty("Provider")]
        public string Provider { get; set; }
    }

    public class AuthorizationDTO
    {
        [JsonProperty("MpRoles")]
        public IDictionary<int, string> MpRoles { get; set; }

        [JsonProperty("OktaRoles")]
        public IDictionary<int, string> OktaRoles { get; set; }
    }

    public class UserInfoDTO
    {
        [JsonProperty("Mp")]
        public MpUserInfoDTO Mp { get; set; }
    }

    public class MpUserInfoDTO
    {
        [JsonProperty("ContactId")]
        public int ContactId { get; set; }

        [JsonProperty("UserId")]
        public int? UserId { get; set; }

        [JsonProperty("ParticipantId")]
        public int? ParticipantId { get; set; }

        [JsonProperty("HouseholdId")]
        public int? HouseholdId { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("DonorId")]
        public int? DonorId { get; set; }
    }
}
