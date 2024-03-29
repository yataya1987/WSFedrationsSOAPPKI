using System.Net.Http;
using System.Threading.Tasks;

#if !DNXCORE50
using System.Web;
#endif

using D2L.Security.OAuth2.Principal;
using D2L.Security.OAuth2.Validation.AccessTokens;

namespace D2L.Security.OAuth2.Validation.Request {
	/// <summary>
	/// An abstraction for authenticating access tokens that works at the request level
	/// rather than the token level (see <see cref="IAccessTokenValidator"/>)
	/// </summary>
	public interface IRequestAuthenticator {
		/// <summary>
		/// Authenticates a token contained in an <see cref="HttpRequestMessage"/>
		/// </summary>
		/// <param name="request">The web request object</param>
		/// <returns>An <see cref="ID2LPrincipal"/> for an authenticated user.</returns>
		Task<ID2LPrincipal> AuthenticateAsync(
			HttpRequestMessage request
		);

#if !DNXCORE50
		/// <summary>
		/// Authenticates a token contained in an <see cref="HttpRequest"/>
		/// </summary>
		/// <param name="request">The web request object.</param>
		/// <returns>An <see cref="ID2LPrincipal"/> for an authenticated user.</returns>
		Task<ID2LPrincipal> AuthenticateAsync(
			HttpRequest request
		);
#endif
	}
}
