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
}
