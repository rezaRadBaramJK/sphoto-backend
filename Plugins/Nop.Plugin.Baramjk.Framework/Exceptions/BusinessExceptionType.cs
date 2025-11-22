using System;

namespace Nop.Plugin.Baramjk.Framework.Exceptions
{
    public readonly struct BusinessExceptionType : IEquatable<BusinessExceptionType>
    {
        private const short BadRequestKey = 400;

        private const short NotFoundKey = 404;

        private const short UnauthorizedKey = 401;

        private const short InternalErrorKey = 500;
        
        private const short BadGatewayKey = 502;

        public static readonly BusinessExceptionType BadRequest = new(BadRequestKey);

        public static readonly BusinessExceptionType NotFound = new(NotFoundKey);

        public static readonly BusinessExceptionType Unauthorized = new(UnauthorizedKey);

        public static readonly BusinessExceptionType InternalError = new(InternalErrorKey);
        
        public static readonly BusinessExceptionType BadGateway = new(BadGatewayKey);

        private readonly short _key;

        private BusinessExceptionType(short key)
        {
            _key = key;
        }

        public static bool operator ==(BusinessExceptionType left, BusinessExceptionType right)
        {
            return left._key == right._key;
        }

        public static bool operator !=(BusinessExceptionType left, BusinessExceptionType right)
        {
            return !(left == right);
        }

        public static implicit operator short(BusinessExceptionType businessExceptionType)
        {
            return businessExceptionType._key;
        }

        public bool Equals(BusinessExceptionType other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;

            if (obj is BusinessExceptionType status) return Equals(status);

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_key);
        }
    }
}