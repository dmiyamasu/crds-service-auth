using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Service.Auth.Exceptions;
using Crossroads.Service.Auth.Models;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using MinistryPlatform.Models;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Interfaces;

namespace Crossroads.Service.Auth.Services
{
    public class MpUserService : IMpUserService
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private IAuthenticationRepository _authenticationRepository;
        private IMinistryPlatformRestRequestBuilderFactory _mpRestBuilder;

        public MpUserService(IAuthenticationRepository authenticationRepository,
                             IMinistryPlatformRestRequestBuilderFactory mpRestBuilder)
        {
            _authenticationRepository = authenticationRepository;
            _mpRestBuilder = mpRestBuilder;
        }

        public int GetMpContactIdFromToken(string token)
        {
            //TODO: See what happens when it can't find a contact Id
            int contactId = _authenticationRepository.GetContactId(token);

            return contactId;
        }

        public MpUserInfoDTO GetMpUserInfoFromContactId(int contactId, string mpAPIToken)
        {
            if (contactId > 0)
            {
                return GetMpUserInfo(contactId, mpAPIToken);
            }
            else
            {
                _logger.Error("No contactId Available for token");
                throw new NoContactIdAvailableException();
            }
        }

        public Dictionary<int, string> GetRoles(string mpAPIToken, int mpContactId)
        {
            // Go get the roles from mp
            var columns = new string[] {
                    "dp_User_Roles.Role_ID",
                    "Role_ID_Table.Role_Name"
                };

            var roles = _mpRestBuilder.NewRequestBuilder()
                                            .WithAuthenticationToken(mpAPIToken)
                                            .WithSelectColumns(columns)
                                      .WithFilter($"User_ID_Table_Contact_ID_Table.[Contact_ID]={mpContactId}")
                                            .Build()
                                            .Search<JObject>("dp_User_Roles");

            var rolesDict = roles.ToDictionary(x => x.Value<int>("Role_ID"), x => x.Value<string>("Role_Name"));

            return rolesDict;
        }

        private MpUserInfoDTO GetMpUserInfo(int contactId, string mpAPIToken)
        {
            var columns = new string[] {
                    "User_Account",
                    "Donor_Record",
                    "Participant_Record",
                    "Email_Address",
                    "Household_ID"
                };

            var contact = _mpRestBuilder.NewRequestBuilder()
                                        .WithAuthenticationToken(mpAPIToken)
                                        .WithSelectColumns(columns)
                                        .Build()
                                        .Get<MpContact>(contactId);

            //TODO: What happens if anything fails?

            MpUserInfoDTO mpUserInfoDTO = new MpUserInfoDTO
            {
                ContactId = contactId,
                UserId = contact.UserAccount,
                ParticipantId = contact.ParticipantRecord,
                HouseholdId = contact.HouseholdId,
                Email = contact.EmailAddress,
                DonorId = contact.DonorRecord
            };

            return mpUserInfoDTO;
        }
    }
}
