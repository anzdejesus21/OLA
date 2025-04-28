using MongoDB.Driver;
using OLA.Common;
using OLA.Configs;
using OLA.Entities;

namespace OLA.Services
{
    public interface IUserService
    {
        Task<User> GetByEmail(string email);
        Task<Response> UpsertAsync(User req);
        Task<Response> DeleteAsync(string id);
    }

    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(
            IDatabaseSettings settings,
            IMongoClient mongoClient
            )
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UserCollectionName);
        }

        public async Task<User> GetByEmail(string email)
        {
            return await _users.Find(x => x.Email == email).FirstOrDefaultAsync();
        }

        public async Task<Response> UpsertAsync(User req)
        {
            try
            {
                if (!string.IsNullOrEmpty(req.Id))
                    await _users.ReplaceOneAsync(x => x.Id.Equals(req.Id), req);
                else
                    await _users.InsertOneAsync(req);

                return new Response { Success = true, Message = "Successfully saved!" };
            }
            catch
            {
                throw;
            }
        }

        public async Task<Response> DeleteAsync(string id)
        {
            try
            {
                await _users.DeleteOneAsync(x => x.Equals(id));
                return new Response { Success = true, Message = "User has been deleted successfully" };
            }
            catch
            {
                throw;
            }
        }
    }
}
