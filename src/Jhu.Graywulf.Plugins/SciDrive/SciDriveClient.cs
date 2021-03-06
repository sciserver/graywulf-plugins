﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Jhu.Graywulf.IO;

namespace Jhu.Graywulf.SciDrive
{
    public class SciDriveClient
    {
        public static SciDriveConfiguration Configuration
        {
            get
            {
                return (SciDriveConfiguration)ConfigurationManager.GetSection("jhu.graywulf/sciDrive");
            }
        }

        public static string GetFilePrefix = "1/files/dropbox/";
        public static string PutFilePrefix = "1/files_put/dropbox/";
        
        /// <summary>
        /// Returns true if the URI points to SciDrive
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool IsUriSciDrive(Uri uri)
        {
            var c = StringComparer.InvariantCultureIgnoreCase;

            // If URI starts with scidrive:/ or when
            // http or https is used but host name equals to config
            if (c.Compare(uri.Scheme, Constants.UriSchemeSciDrive) == 0)
            {
                return true;
            }
            else if (c.Compare(uri.Scheme, Uri.UriSchemeHttp) == 0 ||
                     c.Compare(uri.Scheme, Uri.UriSchemeHttps) == 0)
            {
                // Compare host name with settings
                var uriHost = uri.Host;
                var configHost = SciDriveClient.Configuration?.BaseUri?.Host;

                // If host names match it is a SciDrive instance
                if (configHost != null && c.Compare(uriHost, configHost) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static Uri GetNormalizedUri(Uri uri)
        {
            var c = StringComparer.InvariantCultureIgnoreCase;

            if (c.Compare(uri.Scheme, Constants.UriSchemeSciDrive) == 0)
            {
                // If scidrive: is used as scheme, replace with URI from settings

                return Util.UriConverter.Combine(SciDriveClient.Configuration.BaseUri, uri.PathAndQuery);
            }
            else
            {
                // In any other case just use HTTP or HTTPS
                return uri;
            }
        }

        public static Uri GetFileGetUri(Uri path)
        {
            var uri = Configuration.BaseUri;
            uri = Util.UriConverter.Combine(uri, GetFilePrefix);
            uri = Util.UriConverter.Combine(uri, path);

            return uri;
        }

        public static Uri GetFilePutUri(Uri path)
        {
            var uri = Configuration.BaseUri;
            uri = Util.UriConverter.Combine(uri, PutFilePrefix);
            uri = Util.UriConverter.Combine(uri, path);

            return uri;
        }

        public static Uri GetFilePath(Uri uri)
        {
            var path = Configuration.BaseUri.MakeRelativeUri(uri).ToString();

            if (path.StartsWith(GetFilePrefix))
            {
                path = path.Remove(0, GetFilePrefix.Length);
            }
            else if (path.StartsWith(PutFilePrefix))
            {
                path = path.Remove(0, PutFilePrefix.Length);
            }

            return new Uri(path, UriKind.Relative);
        }


        public static void SetAuthenticationHeaders(Credentials credentials)
        {
            AuthenticationHeader header;

            if (SciDriveClient.Configuration.IsTrustEnabled)
            {
                // TODO: add this once trusts tested and work
                //header = CreateAuthenticationHeaderFromTrust();
                throw new NotImplementedException();
            }
            else
            {
                header = CreateAuthenticationHeaderFromToken();
            }

            credentials.AuthenticationHeaders.Add(header);
        }

        private static AuthenticationHeader CreateAuthenticationHeaderFromToken()
        {
            // TODO: can be rewritten to use evidence from identity as token
            // instead of looking it up in the cache

            var name = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            Keystone.Token token;

            if (Keystone.KeystoneTokenCache.Instance.TryGetValueByUserName(name, name, out token))
            {
                var header = new AuthenticationHeader()
                {
                    Name = Web.Security.KeystoneAuthentication.Configuration.AuthTokenHeader,
                    Value = token.ID
                };

                return header;
            }
            else
            {
                throw new System.Security.SecurityException("Keystone token required");
            }
        }

        private static AuthenticationHeader CreateAuthenticationHeaderFromTrust()
        {
            var name = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            Keystone.Token token;

            // TODO: replace keystone token to a trust here

            if (Keystone.KeystoneTokenCache.Instance.TryGetValueByUserName(name, name, out token))
            {
                // Not we have a Keystone token but it needs to be exchanged for a longer-lived
                // trust for batch processing

                var ksclient = new Keystone.KeystoneClient()
                {
                    UserCredentials = new Keystone.KeystoneCredentials()
                    {
                        TokenID = token.ID
                    }
                };

                var trust = new Keystone.Trust()
                {
                    ExpiresAt = DateTime.UtcNow + Configuration.TrustExpiresAfter,
                    ProjectID = token.Project.ID,
                    Impersonation = true,
                    TrustorUserID = token.User.ID,
                    //TrusteeUserID = token.User.ID,
                    TrusteeUserID = "", // TODO: whom to trust?
                    RemainingUses = 5,
                    Roles = new Keystone.Role[]
                    {
                        new Keystone.Role()
                        {
                            // TODO: is specifying name enough?
                            Name = Configuration.TrustRole
                        }
                    }
                };

                trust = ksclient.Create(trust);

                var header = new AuthenticationHeader()
                {
                    Name = Web.Security.KeystoneAuthentication.Configuration.AuthTokenHeader,
                    Value = trust.ID
                };

                return header;
            }
            else
            {
                throw new System.Security.SecurityException("Keystone token required");
            }
        }
    }
}
