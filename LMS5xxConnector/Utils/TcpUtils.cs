using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text;

namespace LMS5xxConnector.Utils
{
    internal static class TcpUtils
    {
#if NETSTANDARD2_0
        public static bool TryParseEndpoint(ReadOnlySpan<char> value, out IPEndPoint? result)
#endif
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        public static bool TryParseEndpoint(ReadOnlySpan<char> value, [NotNullWhen(true)] out IPEndPoint? result)
#endif
        {
            var addressLength = value.Length;
            var lastColonPos = value.LastIndexOf(':');

            if (lastColonPos > 0)
            {
                if (value[lastColonPos - 1] == ']')
                    addressLength = lastColonPos;

                else if (value.Slice(0, lastColonPos).LastIndexOf(':') == -1)
                    addressLength = lastColonPos;
            }

            if (IPAddress.TryParse(value.Slice(0, addressLength).ToString(), out var address))
            {
                var port = 502U;

                if (addressLength == value.Length ||
                    (uint.TryParse(value.Slice(addressLength + 1).ToString(), NumberStyles.None,
                        CultureInfo.InvariantCulture, out port) && port <= 65536))

                {
                    result = new IPEndPoint(address, (int)port);
                    return true;
                }
            }

            result = default;

            return false;
        }
    }
}
