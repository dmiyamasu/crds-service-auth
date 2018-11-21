using System;
using Xunit;
using Moq;
using Crossroads.Service.Auth.Interfaces;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Service.Auth.Services;

namespace Crossroads.Service.Auth.Tests
{
    public class OktaUserServiceTest
    {
        public OktaUserServiceTest()
        {
            
        }

        [Fact]
        public void GetMpContactIdFromDecodedToken_ReturnsContactId_WhenValid()
        {

        }

        [Fact]
        public void GetMpContactIdFromDecodedToken_ThrowsException_NoMpContactId()
        {

        }

        [Fact]
        public void GetRoles_ReturnsEmptyDict_WhenNoRoles()
        {

        }

        [Fact]
        public void GetRoles_ReturnsDict_WhenRolesAvailable()
        {

        }
    }
}
