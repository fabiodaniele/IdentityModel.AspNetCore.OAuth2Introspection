﻿// Copyright (c) Dominick Baier & Brock Allen. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authentication;
using IdentityModel.Client;
using IdentityModel.AspNetCore.OAuth2Introspection.Infrastructure;
using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Options class for the OAuth 2.0 introspection endpoint authentication handler
    /// </summary>
    public class OAuth2IntrospectionOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Sets the base-path of the token provider.
        /// If set, the OpenID Connect discovery document will be used to find the introspection endpoint.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Sets the URL of the introspection endpoint.
        /// If set, Authority is ignored.
        /// </summary>
        public string IntrospectionEndpoint { get; set; }

        /// <summary>
        /// Sets the URL of the userinfo endpoint.
        /// If set, Authority is ignored.
        /// </summary>
        public string UserinfoEndpoint { get; set; }

        /// <summary>
        /// Specifies the id of the introspection client.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Specifies the shared secret of the introspection client.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Specifies the claim type to use for the name claim (defaults to 'name')
        /// </summary>
        public string NameClaimType { get; set; } = "name";

        /// <summary>
        /// Specifies the claim type to use for the role claim (defaults to 'role')
        /// </summary>
        public string RoleClaimType { get; set; } = "role";

        /// <summary>
        /// Specifies the policy for the discovery client
        /// </summary>
        public DiscoveryPolicy DiscoveryPolicy { get; set; } = new DiscoveryPolicy();

        /// <summary>
        /// Specifies the timout for contacting the discovery endpoint
        /// </summary>
        public TimeSpan DiscoveryTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Specifies the HTTP handler for the discovery endpoint
        /// </summary>
        public HttpMessageHandler DiscoveryHttpHandler { get; set; }

        /// <summary>
        /// Specifies the timeout for contacting the introspection endpoint
        /// </summary>
        public TimeSpan IntrospectionTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Specifies the HTTP handler for the introspection endpoint
        /// </summary>
        public HttpMessageHandler IntrospectionHttpHandler { get; set; }

        /// <summary>
        /// Specifies the timeout for contacting the userinfo endpoint
        /// </summary>
        public TimeSpan UserinfoTimeout { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Specifies the HTTP handler for the userinfo endpoint
        /// </summary>
        public HttpMessageHandler UserinfoHttpHandler { get; set; }

        /// <summary>
        /// Specifies whether tokens that contain dots (most likely a JWT) are skipped
        /// </summary>
        public bool SkipTokensWithDots { get; set; } = true;

        /// <summary>
        /// Specifies whether the token should be stored
        /// </summary>
        public bool SaveToken { get; set; } = true;

        /// <summary>
        /// Specifies whether to get claims from userinfo endpoint and add them to the authentication ticket. Default is false.
        /// </summary>
        public bool GetClaimsFromUserinfoEndpoint { get; set; } = false;

        /// <summary>
        /// Specifies whether to try to split the value of the role claim, for use with IPs sending all roles in a single claim. Default is false.
        /// </summary>
        public bool RoleClaimValueSplitting { get; set; } = false;

        /// <summary>
        /// Specifies the character to be used for splitting the role claim value, if RoleClaimValueSplitting is set to "true". Default is ','.
        /// </summary>
        public char RoleClaimValueSplittingChar { get; set; } = ',';


        /// <summary>
        /// Specifies whether the outcome of the toke validation should be cached. This reduces the load on the introspection endpoint at the STS
        /// </summary>
        public bool EnableCaching { get; set; } = false;

        /// <summary>
        /// Specifies for how long the outcome of the token validation should be cached.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Specifies the method how to retrieve the token from the HTTP request
        /// </summary>
        public Func<HttpRequest, string> TokenRetriever { get; set; } = TokenRetrieval.FromAuthorizationHeader();

        internal AsyncLazy<IntrospectionClient> IntrospectionClient { get; set; }
        internal ConcurrentDictionary<string, AsyncLazy<IntrospectionResponse>> LazyIntrospections { get; set; }

        internal AsyncLazy<UserInfoClient> UserinfoClient { get; set; }
        internal ConcurrentDictionary<string, AsyncLazy<UserInfoResponse>> LazyUserinfos { get; set; }

        /// <summary>
        /// Check that the options are valid. Should throw an exception if things are not ok.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// You must either set Authority or IntrospectionEndpoint
        /// or
        /// You must either set a ClientId or set an introspection HTTP handler
        /// </exception>
        /// <exception cref="ArgumentException">TokenRetriever must be set - TokenRetriever</exception>
        public override void Validate()
        {
            base.Validate();

            if (Authority.IsMissing() && IntrospectionEndpoint.IsMissing())
            {
                throw new InvalidOperationException("You must either set Authority or IntrospectionEndpoint");
            }

            if (ClientId.IsMissing() && IntrospectionHttpHandler == null)
            {
                throw new InvalidOperationException("You must either set a ClientId or set an introspection HTTP handler");
            }

            if (GetClaimsFromUserinfoEndpoint)
            {
                if (Authority.IsMissing() && UserinfoEndpoint.IsMissing())
                {
                    throw new InvalidOperationException("You must either set Authority or UserinfoEndpoint");
                }
            }

            if (TokenRetriever == null)
            {
                throw new ArgumentException("TokenRetriever must be set", nameof(TokenRetriever));
            }
        }
    }
}
