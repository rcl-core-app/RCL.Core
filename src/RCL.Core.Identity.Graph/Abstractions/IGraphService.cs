#nullable disable

using Microsoft.Graph;

namespace RCL.Core.Identity.Graph
{
    public interface IGraphService
    {
        Task<User> CreateUser(User user);
        Task<User> GetUserByPrincipalNameAsync(string principalName);
        Task<User> GetUserByObjectIdAsync(string objectId);
        Task<User> GetUserByEmailAsync(string email);
        Task<UserClaimsData> GetUserClaimsDataByObjectIdAsync(string objectId);
        Task<UserClaimsData> GetUserClaimsDataByEmailAsync(string email);
        Task<List<string>> GetUserInGroupsByObjectIdAsync(string objectId);
        Task<List<User>> GetUsersByNameAsync(string givenName, string surname);
        Dictionary<string, object> GetUserCustomAttributes(User user);
        Task<UserClaimsData> UpdateUserAsync(UserClaimsData userClaimsData);
        Task<User> UpdateUserPhotoAsync(string objectId, string photoUrl);
        Task DeleteUserByObjectIdAsync(string objectId);
    }
}
