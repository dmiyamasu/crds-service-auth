using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Crossroads.Service.Auth.Models
{
    public class AuthDTO
    {
        [JsonProperty("authentication")]
        public AuthenticationDTO authentication { get; set; }

        [JsonProperty("authorization")]
        public AuthorizationDTO authorization { get; set; }

        [JsonProperty("userInfo")]
        public UserInfoDTO userInfo { get; set; }
    }

    public class AuthenticationDTO
    {
        [JsonProperty("authenticated")]
        public bool authenticated { get; set; }

        [JsonProperty("provider")]
        public string provider { get; set; }

        [JsonProperty("message")]
        public string message { get; set; }
    }

    public class AuthorizationDTO
    {
        [JsonProperty("mpRoles")]
        public IEnumerable<string> mpRoles { get; set; }
    }

    public class UserInfoDTO
    {
        [JsonProperty("mpContactId")]
        public int mpContactId { get; set; }

        [JsonProperty("mpUserId")]
        public int? mpUserId { get; set; }

        [JsonProperty("mpParticipantId")]
        public int? mpParticipantId { get; set; }

        [JsonProperty("mpHouseholdId")]
        public int? mpHouseholdId { get; set; }

        [JsonProperty("mpEmail")]
        public string mpEmail { get; set; }

        [JsonProperty("mpDonorId")]
        public int? mpDonorId { get; set; }
    }
}
