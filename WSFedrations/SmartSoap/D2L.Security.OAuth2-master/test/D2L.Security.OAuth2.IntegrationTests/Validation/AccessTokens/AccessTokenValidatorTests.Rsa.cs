﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using D2L.Security.OAuth2.Keys;
using D2L.Security.OAuth2.TestFramework;
using D2L.Security.OAuth2.Validation.Exceptions;
using D2L.Services;
using NUnit.Framework;

namespace D2L.Security.OAuth2.Validation.AccessTokens {
	internal sealed partial class AccessTokenValidatorTests {
		[TestFixture]
		internal sealed class Rsa {

			private AuthServiceMock m_authService;
			private IAccessTokenValidator m_accessTokenValidator;

			[OneTimeSetUp]
			public void TestFixtureSetUp() {
				m_authService = new AuthServiceMock();
				m_accessTokenValidator = AccessTokenValidatorFactory.CreateRemoteValidator(
					new HttpClient(),
					new Uri( m_authService.Host, ".well-known/jwks" )
				);

				m_authService.SetupJwks().Wait();
			}

			[OneTimeTearDown]
			public void TestFixtureTearDown() {
				m_authService.SafeDispose();
			}

			[Test]
			public async Task ValidateAsync_GoodSignature_Succeeds() {
				const string SUBJECT = "123";
				string token = await m_authService
					.SignTokenBackdoor( new UnsignedToken(
						"fake issuer",
						"fake audience",
						new Dictionary<string, object> { { "sub", SUBJECT } },
						DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
						DateTime.UtcNow + TimeSpan.FromHours( 1 ) ) )
					.SafeAsync();

				IAccessToken accessToken = await m_accessTokenValidator
					.ValidateAsync( token )
					.SafeAsync();

				Assert.IsNotNull( accessToken );
				accessToken.Claims.TryGetClaim( "sub", out string subject );
				accessToken.Claims.TryGetClaim( "fakeclaim", out string fakeclaim );

				Assert.AreEqual( SUBJECT, subject );
				Assert.IsNull( fakeclaim );
			}

			[Test]
			public async Task ValidateAsync_BadSignature_Fails() {
				string token = await m_authService
					.SignTokenBackdoor( new UnsignedToken(
						"fake issuer",
						"fake audience",
						new Dictionary<string, object>(),
						DateTime.UtcNow - TimeSpan.FromSeconds( 1 ),
						DateTime.UtcNow + TimeSpan.FromHours( 1 ) ) )
					.SafeAsync();

				token += "abcd";

				Assert.Throws<ValidationException>( () => {
					var response = m_accessTokenValidator
						.ValidateAsync( token )
						.SafeAsync()
						.GetAwaiter()
						.GetResult();
				} );
			}

			[Test]
			public void ValidateAsync_KeyIdNotInAuthService_Fails() {
				// This JWT has a keyId that doesn't match the one in the auth service
				string jwtWithBadKeyId = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.dUQ2bB3anqRmI-wnC4bulmnwo7wAdrvXo3hn3Dp0tuUl01dy2FhsJESJ9BZ2BeykrLRv2EgdbTW3BCBpBqLbrKQaG_XuGX5MrtXFwHE7i9wWmDsetlJn_cvsZlhPg-voI2iGqT-gpiE9GfWcXjTPUCxAbz6Pqepi0-JDS9uTrCg";

				var e = Assert.Throws<PublicKeyNotFoundException>( () => {
					var response = m_accessTokenValidator
						.ValidateAsync( jwtWithBadKeyId )
						.SafeWait();
				} );

				StringAssert.Contains( "00000000-0000-0000-0000-000000000000", e.Message );
				StringAssert.Contains( m_authService.Host.AbsoluteUri, e.Message );
			}
		}
	}
}
