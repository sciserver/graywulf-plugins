﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DotNetOpenAuth.OpenId.RelyingParty;
using DotNetOpenAuth.OpenId.Extensions.AttributeExchange;
using Jhu.Graywulf.Registry;

namespace Jhu.Graywulf.Web.Security
{
    /// <summary>
    /// Implements OpenID authentication.
    /// </summary>
    public class OpenIDAuthenticator : IInteractiveAuthenticator
    {
        private string authorityName;
        private string authorityUri;
        private string displayName;
        private string discoveryUrl;

        /// <summary>
        /// Gets or sets the name of the authority
        /// </summary>
        [XmlElement]
        public string AuthorityName
        {
            get { return authorityName; }
            set { authorityName = value; }
        }

        /// <summary>
        /// Gets or sets the URI uniquely identifying the authority
        /// </summary>
        [XmlElement]
        public string AuthorityUri
        {
            get { return authorityUri; }
            set { authorityUri = value; }
        }

        /// <summary>
        /// Gets or sets the display name of the authority
        /// </summary>
        [XmlElement]
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }

        /// <summary>
        /// Gets or set the discovery URL of the authority.
        /// </summary>
        [XmlElement]
        public string DiscoveryUrl
        {
            get { return discoveryUrl; }
            set { discoveryUrl = value; }
        }

        /// <summary>
        /// Gets the name of the authentication protocol.
        /// </summary>
        [XmlIgnore]
        public string Protocol
        {
            get { return Constants.ProtocolNameOpenID; }
        }

        public OpenIDAuthenticator()
        {
            InitializeMembers();
        }

        private void InitializeMembers()
        {
            this.authorityName = null;
            this.authorityUri = null;
            this.displayName = null;
            this.discoveryUrl = null;
        }

        /// <summary>
        /// Authenicates a user based on the information in the HTTP request.
        /// </summary>
        /// <returns></returns>
        public GraywulfPrincipal Authenticate()
        {
            // Get OpenID provider's response from the http context
            using (var openid = new OpenIdRelyingParty())
            {
                var response = openid.GetResponse();

                // TODO: figure out which OpenID provider sent the response
                // and associate with the right authenticator

                if (response != null)
                {
                    switch (response.Status)
                    {
                        case AuthenticationStatus.Authenticated:
                            return CreatePrincipal(response);
                        case AuthenticationStatus.Canceled:
                        case AuthenticationStatus.Failed:
                            throw new System.Security.Authentication.AuthenticationException("OpenID authentication failed.", response.Exception);
                        case AuthenticationStatus.ExtensionsOnly:
                        case AuthenticationStatus.SetupRequired:
                            throw new InvalidOperationException();
                        default:
                            throw new NotImplementedException();
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Redirects the browser to the sign in page of the OpenID authority.
        /// </summary>
        public void RedirectToLoginPage()
        {
            using (var openid = new OpenIdRelyingParty())
            {
                // TODO: do this only once when OpenID is initialized
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;

                IAuthenticationRequest request = openid.CreateRequest(discoveryUrl);
                request.AddExtension(CreateFetchRequest());
                request.RedirectToProvider();
            }
        }

        /// <summary>
        /// Creates a fetch request by requesting all interesting fields.
        /// </summary>
        /// <returns></returns>
        private FetchRequest CreateFetchRequest()
        {
            var fetch = new FetchRequest();

            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.Prefix, false));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.First, false));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.Middle, false));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.Last, false));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Name.FullName, false));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Person.Gender, false));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Email, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.BirthDate.WholeBirthDate, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Company.CompanyName, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Company.JobTitle, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.WorkAddress.StreetAddressLine1, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.WorkAddress.StreetAddressLine2, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.WorkAddress.State, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.WorkAddress.City, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.WorkAddress.Country, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.WorkAddress.PostalCode, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Phone.Work, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Phone.Home, true));
            fetch.Attributes.Add(new AttributeRequest(WellKnownAttributes.Contact.Phone.Mobile, true));

            // TODO: add more

            return fetch;
        }

        /// <summary>
        /// Creates a Graywulf principal from the authentication response.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <remarks>
        /// The function also creates and initializer a registry user object by
        /// filling in the available information. The user is not save to the
        /// registry.
        /// </remarks>
        private GraywulfPrincipal CreatePrincipal(IAuthenticationResponse response)
        {
            var identity = new GraywulfIdentity()
            {
                Protocol = this.Protocol,
                AuthorityName = this.AuthorityName,
                AuthorityUri = response.Provider.Uri.ToString(),
                Identifier = response.ClaimedIdentifier,
                IsAuthenticated = false,
                User = new User()
            };

            var fetch = response.GetExtension<FetchResponse>();

            if (fetch.Attributes.Contains(WellKnownAttributes.Contact.Email))
            {
                identity.User.Name = Jhu.Graywulf.Util.EmailFormatter.ToUsername(fetch.Attributes[WellKnownAttributes.Contact.Email].Values[0]);
            }

            identity.User.Title = fetch.Attributes.Contains(WellKnownAttributes.Name.Prefix) ? fetch.Attributes[WellKnownAttributes.Name.Prefix].Values[0] : "";
            identity.User.FirstName = fetch.Attributes.Contains(WellKnownAttributes.Name.First) ? fetch.Attributes[WellKnownAttributes.Name.First].Values[0] : "";
            identity.User.MiddleName = fetch.Attributes.Contains(WellKnownAttributes.Name.Middle) ? fetch.Attributes[WellKnownAttributes.Name.Middle].Values[0] : "";
            identity.User.LastName = fetch.Attributes.Contains(WellKnownAttributes.Name.Last) ? fetch.Attributes[WellKnownAttributes.Name.Last].Values[0] : "";
            // TODO identity.User.Gender = fetch.Attributes.Contains(WellKnownAttributes.Person.Gender) ?fetch.Attributes[WellKnownAttributes.Person.Gender].Values[0];
            identity.User.Email = fetch.Attributes.Contains(WellKnownAttributes.Contact.Email) ? fetch.Attributes[WellKnownAttributes.Contact.Email].Values[0] : "";
            identity.User.DateOfBirth = fetch.Attributes.Contains(WellKnownAttributes.BirthDate.WholeBirthDate) ? DateTime.Parse(fetch.Attributes[WellKnownAttributes.BirthDate.WholeBirthDate].Values[0]) : new DateTime(1950, 1, 1);
            identity.User.Company = fetch.Attributes.Contains(WellKnownAttributes.Company.CompanyName) ? fetch.Attributes[WellKnownAttributes.Company.CompanyName].Values[0] : "";
            identity.User.JobTitle = fetch.Attributes.Contains(WellKnownAttributes.Company.JobTitle) ? fetch.Attributes[WellKnownAttributes.Company.JobTitle].Values[0] : "";
            identity.User.Address = fetch.Attributes.Contains(WellKnownAttributes.Contact.WorkAddress.StreetAddressLine1) ? fetch.Attributes[WellKnownAttributes.Contact.WorkAddress.StreetAddressLine1].Values[0] : "";
            identity.User.Address2 = fetch.Attributes.Contains(WellKnownAttributes.Contact.WorkAddress.StreetAddressLine2) ? fetch.Attributes[WellKnownAttributes.Contact.WorkAddress.StreetAddressLine2].Values[0] : "";
            identity.User.State = fetch.Attributes.Contains(WellKnownAttributes.Contact.WorkAddress.State) ? fetch.Attributes[WellKnownAttributes.Contact.WorkAddress.State].Values[0] : "";
            identity.User.City = fetch.Attributes.Contains(WellKnownAttributes.Contact.WorkAddress.City) ? fetch.Attributes[WellKnownAttributes.Contact.WorkAddress.City].Values[0] : "";
            identity.User.Country = fetch.Attributes.Contains(WellKnownAttributes.Contact.WorkAddress.Country) ? fetch.Attributes[WellKnownAttributes.Contact.WorkAddress.Country].Values[0] : "";
            identity.User.ZipCode = fetch.Attributes.Contains(WellKnownAttributes.Contact.WorkAddress.PostalCode) ? fetch.Attributes[WellKnownAttributes.Contact.WorkAddress.PostalCode].Values[0] : "";
            identity.User.WorkPhone = fetch.Attributes.Contains(WellKnownAttributes.Contact.Phone.Work) ? fetch.Attributes[WellKnownAttributes.Contact.Phone.Work].Values[0] : "";
            identity.User.HomePhone = fetch.Attributes.Contains(WellKnownAttributes.Contact.Phone.Home) ? fetch.Attributes[WellKnownAttributes.Contact.Phone.Home].Values[0] : "";
            identity.User.CellPhone = fetch.Attributes.Contains(WellKnownAttributes.Contact.Phone.Mobile) ? fetch.Attributes[WellKnownAttributes.Contact.Phone.Mobile].Values[0] : "";

            return new GraywulfPrincipal(identity);
        }
    }
}
