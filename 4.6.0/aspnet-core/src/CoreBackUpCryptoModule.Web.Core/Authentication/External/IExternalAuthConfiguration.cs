using System.Collections.Generic;

namespace CoreBackUpCryptoModule.Authentication.External
{
    public interface IExternalAuthConfiguration
    {
        List<ExternalLoginProviderInfo> Providers { get; }
    }
}
