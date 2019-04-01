using System;

namespace Crossroads.Service.Auth.Exceptions
{
    public class TokenMalformedException : Exception
    {
        public TokenMalformedException() : base("The token was malformatted - unable to decode")
        {

        }

        public TokenMalformedException(string message) : base(message)
        {
            
        }
    }

    public class NoContactIdAvailableException : Exception
    {
        public NoContactIdAvailableException() : base("Unable to obtain contactId")
        {

        }

        public NoContactIdAvailableException(string message) : base(message)
        {

        }
    }

    public class InvalidNumberOfResultsForMpContact : Exception
    {
        public InvalidNumberOfResultsForMpContact() : base("Invalid number of results for mp contact id")
        {

        }

        public InvalidNumberOfResultsForMpContact(string message) : base(message)
        {

        }
    }

    public class ConfigurationSigningKeysIsNull : Exception
    {
        public ConfigurationSigningKeysIsNull() : base("The signing keys associated with the issuer oauth configuration are null")
        {

        }

        public ConfigurationSigningKeysIsNull(string message) : base(message)
        {

        }
    }
}
