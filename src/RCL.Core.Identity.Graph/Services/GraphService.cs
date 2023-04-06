#nullable disable

using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;

namespace RCL.Core.Identity.Graph
{
    internal class GraphService : IGraphService
    {
        private GraphServiceClient _graphClient = null;
        private readonly IOptions<GraphOptions> _options;

        public GraphService(IOptions<GraphOptions> options)
        {
            _options = options;
        }

        private void GetGraphServiceClient()
        {
            DelegateAuthenticationProvider del = new DelegateAuthenticationProvider(AuthenticationProvider);
            _graphClient = new GraphServiceClient(del);
        }

        public async Task<User> CreateUser(User user)
        {
            try
            {
                GetGraphServiceClient();
                User createdUser = await _graphClient.Users
                .Request()
                .AddAsync(user);

                OpenTypeExtension extension = new OpenTypeExtension
                {
                    ExtensionName = "DigitalIdentity-Custom-Extensions",
                    AdditionalData = user.AdditionalData
                };

                await _graphClient.Users[createdUser.Id].Extensions.Request()
                    .AddAsync(extension);

                return createdUser;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> GetUserByPrincipalNameAsync(string principalName)
        {
            try
            {
                GetGraphServiceClient();
                User user = await _graphClient.Users[principalName]
                    .Request()
                    .Select(e => new
                    {
                        e.GivenName,
                        e.Surname,
                        e.StreetAddress,
                        e.City,
                        e.State,
                        e.PostalCode,
                        e.Country,
                        e.JobTitle,
                        e.UserPrincipalName,
                        e.DisplayName,
                        e.Id,
                        e.AdditionalData
                    })
                    .GetAsync();

                var extensions = await _graphClient.Users[user.Id].Extensions.Request().GetAsync();
                user.Extensions = extensions;

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> GetUserByObjectIdAsync(string objectId)
        {
            try
            {
                GetGraphServiceClient();
                var user = await _graphClient.Users[objectId]
                .Request()
                .Select($"id,givenName,surname,streetAddress,city,state,postalCode,country,jobTitle,userPrincipalName,displayName,identities,{GetExtensions().Item1},{GetExtensions().Item2},{GetExtensions().Item3},{GetExtensions().Item4}")
                .GetAsync();

                return user;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            try
            {
                GetGraphServiceClient();
                var user = await _graphClient.Users
                .Request()
                .Filter($"identities/any(id:id/issuer eq 'rclb2c.onmicrosoft.com' and id/issuerAssignedId eq '{email}')")
                .Select($"id,givenName,surname,streetAddress,city,state,postalCode,country,jobTitle,userPrincipalName,displayName,identities,{GetExtensions().Item1},{GetExtensions().Item2},{GetExtensions().Item3},{GetExtensions().Item4}")
                .GetAsync();

                if (user?.Count > 0)
                {
                    return user.FirstOrDefault();
                }

                return new User();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserClaimsData> GetUserClaimsDataByObjectIdAsync(string objectId)
        {
            try
            {
                User user = await GetUserByObjectIdAsync(objectId);

                if (!string.IsNullOrEmpty(user?.Id))
                {
                    return GetUserClaimsData(user);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return new UserClaimsData();
        }

        public async Task<UserClaimsData> GetUserClaimsDataByEmailAsync(string email)
        {
            try
            {
                User user = await GetUserByEmailAsync(email);

                if (!string.IsNullOrEmpty(user?.Id))
                {
                    return GetUserClaimsData(user);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return new UserClaimsData();
        }

        public async Task<UserClaimsData> UpdateUserAsync(UserClaimsData userClaimsData)
        {
            try
            {
                GetGraphServiceClient();

                User user = new User
                {
                    GivenName = userClaimsData.GivenName,
                    Surname = userClaimsData.SurName,
                    DisplayName = userClaimsData.DisplayName,
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {$"extension_{_options.Value.ExtensionAppId.Replace("-","")}_PhotoUrl", userClaimsData.PhotoUrl},
                        {$"extension_{_options.Value.ExtensionAppId.Replace("-","")}_DateofBirth", Convert.ToDateTime(userClaimsData.DateOfBirth).ToString("dd/MM/yyyy")},
                        {$"extension_{_options.Value.ExtensionAppId.Replace("-","")}_DateVerified", Convert.ToDateTime(userClaimsData.DateVerified).ToString("dd/MM/yyyy")},
                        {$"extension_{_options.Value.ExtensionAppId.Replace("-","")}_Userisverified", userClaimsData.UserIsVerified},
                    }
                };

                await _graphClient.Users[userClaimsData.ObjectId]
                .Request()
                .UpdateResponseAsync(user);

                return await GetUserClaimsDataByObjectIdAsync(userClaimsData.ObjectId);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> UpdateUserPhotoAsync(string objectId, string photoUrl)
        {
            try
            {
                GetGraphServiceClient();

                User user = new User
                {
                    AdditionalData = new Dictionary<string, object>()
                    {
                        {$"extension_{_options.Value.ExtensionAppId.Replace("-","")}_PhotoUrl", photoUrl},
                    }
                };

                await _graphClient.Users[objectId]
                .Request()
                .UpdateResponseAsync(user);

                return await GetUserByObjectIdAsync(objectId);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<User>> GetUsersByNameAsync(string givenName, string surname)
        {
            try
            {
                GetGraphServiceClient();

                List<User> users = new List<User>();

                var page = await _graphClient.Users
                    .Request()
                    .Filter($"GivenName eq '{givenName}' and Surname eq '{surname}'")
                    .Select(e => new
                    {
                        e.GivenName,
                        e.Surname,
                        e.StreetAddress,
                        e.City,
                        e.State,
                        e.PostalCode,
                        e.Country,
                        e.JobTitle,
                        e.UserPrincipalName,
                        e.DisplayName,
                        e.Id,
                        e.AdditionalData
                    })
                    .GetAsync();

                if (page.ToList().Count > 0)
                {
                    foreach (var user in page.ToList())
                    {
                        var extensions = await _graphClient.Users[user.Id].Extensions.Request().GetAsync();
                        user.Extensions = extensions;

                        users.Add(user);
                    }
                }

                return users;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetUserInGroupsByObjectIdAsync(string objectId)
        {
            try
            {
                GetGraphServiceClient();
                var page = await _graphClient.Users[objectId]
                    .MemberOf
                    .Request()
                    .GetAsync();

                var groupNames = new List<string>();
                groupNames.AddRange(page
                        .OfType<Group>()
                        .Select(x => x.DisplayName)
                        .Where(name => !string.IsNullOrEmpty(name)));
                while (page.NextPageRequest != null)
                {
                    page = await page.NextPageRequest.GetAsync();
                    groupNames.AddRange(page
                        .OfType<Group>()
                        .Select(x => x.DisplayName)
                        .Where(name => !string.IsNullOrEmpty(name)));
                }

                return groupNames;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public Dictionary<string, object> GetUserCustomAttributes(User user)
        {
            Dictionary<string, object> customAttributes = new Dictionary<string, object>();

            try
            {
                if (user?.Extensions?.CurrentPage?.Count > 0)
                {
                    var extension = user.Extensions.CurrentPage.FirstOrDefault();

                    if (extension?.AdditionalData?.Count > 0)
                    {
                        customAttributes = (Dictionary<string, object>)extension.AdditionalData;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return customAttributes;
        }

        public async Task DeleteUserByObjectIdAsync(string objectId)
        {
            try
            {
                GetGraphServiceClient();
                await _graphClient.Users[objectId]
                .Request()
                .DeleteAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task AuthenticationProvider(HttpRequestMessage requestMessage)
        {
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create(_options.Value.AuthClientId)
            .WithTenantId(_options.Value.AuthTenantId)
            .WithClientSecret(_options.Value.AuthClientSecret)
            .Build();

            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            var authResult = await confidentialClientApplication
               .AcquireTokenForClient(scopes)
               .ExecuteAsync();

            requestMessage.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
        }

        private (string, string, string, string) GetExtensions()
        {
            string ext1 = $"extension_{_options.Value.ExtensionAppId.Replace("-", "")}_PhotoUrl";
            string ext2 = $"extension_{_options.Value.ExtensionAppId.Replace("-", "")}_DateofBirth";
            string ext3 = $"extension_{_options.Value.ExtensionAppId.Replace("-", "")}_Userisverified";
            string ext4 = $"extension_{_options.Value.ExtensionAppId.Replace("-", "")}_DateVerified";

            return (ext1, ext2, ext3, ext4);
        }

        private UserClaimsData GetUserClaimsData(User user)
        {
            UserClaimsData userClaimsData = new UserClaimsData
            {
                ObjectId = user?.Id ?? string.Empty,
                GivenName = user?.GivenName ?? string.Empty,
                SurName = user?.Surname ?? string.Empty,
                DisplayName = user?.DisplayName ?? string.Empty,
                StreetAddress = user?.StreetAddress ?? string.Empty,
                City = user?.City ?? string.Empty,
                State = user?.State ?? string.Empty,
                PostalCode = user?.PostalCode ?? string.Empty,
                Country = user?.Country ?? string.Empty,
                JobTitle = user?.JobTitle ?? string.Empty
            };

            var identity = user?.Identities?.Where(w => w.SignInType == "emailAddress").FirstOrDefault();

            if (identity != null)
            {
                userClaimsData.Email = identity?.IssuerAssignedId ?? string.Empty;
            }

            Dictionary<string, object> keyValuePairs = (Dictionary<string, object>)user?.AdditionalData;

            if (keyValuePairs != null)
            {
                object valPhotoUrl = null;
                object valDoB = null;
                object valIsVerified = null;
                object valDV = null;

                bool photoUrlClaim = keyValuePairs.TryGetValue($"extension_{_options.Value.ExtensionAppId.Replace("-", "")}_PhotoUrl", out valPhotoUrl);

                if (valPhotoUrl != null)
                {
                    userClaimsData.PhotoUrl = valPhotoUrl?.ToString() ?? string.Empty;
                }

                bool dateofBirthClaim = keyValuePairs.TryGetValue($"extension_{_options.Value.ExtensionAppId.Replace("-", "")}_DateofBirth", out valDoB);

                if (valDoB != null)
                {
                    userClaimsData.DateOfBirth = DateTime.ParseExact(valDoB.ToString(), "dd/MM/yyyy", null); 
                }

                bool dateVerifiedClaim = keyValuePairs.TryGetValue($"extension_{_options.Value.ExtensionAppId.Replace("-", "")}_DateVerified", out valDV);

                if (valDV != null)
                {
                    userClaimsData.DateVerified = DateTime.ParseExact(valDV.ToString(), "dd/MM/yyyy", null); 
                }

                bool isVerifiedClaim = keyValuePairs.TryGetValue($"extension_{_options.Value.ExtensionAppId.Replace("-", "")}_Userisverified", out valIsVerified);

                if (valIsVerified != null)
                {
                    if(valIsVerified.ToString() == "True")
                    {
                        userClaimsData.UserIsVerified = true;
                    }
                    else
                    {
                        userClaimsData.UserIsVerified = false;
                    }
                    
                }
            }

            return userClaimsData;
        }
    }
}
