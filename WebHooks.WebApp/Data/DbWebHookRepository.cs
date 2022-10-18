using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using System.Text.Json;
using System.Globalization;
using WebHooks.WebApp.Model;

namespace WebHooks.WebApp.Data
{
    public class DbWebHookRepository : IWebHookRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DbWebHookRepository> _logger;

        public DbWebHookRepository(ApplicationDbContext context, ILogger<DbWebHookRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ICollection<WebHook>> GetAllWebHooksAsync(string userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }

            userId = NormalizeKey(userId);

            try
            {

                var registrations = await _context.Set<Registration>().Where(r => r.User == userId).ToArrayAsync();
                ICollection<WebHook> result = registrations.Select(r => ConvertToWebHook(r))
                    .Where(w => w != null)
                    .ToArray()!;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public async Task<WebHook?> LookupWebHookAsync(string userId, string id)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            userId = NormalizeKey(userId);
            id = NormalizeKey(id);

            try
            {
                var registration = await _context.Set<Registration>().Where(r => r.User == userId && r.Id == id)
                    .FirstOrDefaultAsync();
                if (registration != null)
                {
                    return ConvertToWebHook(registration);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        public async Task<ICollection<WebHook>> QueryWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            try
            {
                var registrations = await _context.Set<Registration>().Where(r => r.User == user).ToArrayAsync();
                ICollection<WebHook> matches = registrations.Select(r => ConvertToWebHook(r))
                    .Where(w => w !=null).ToArray()!;
                return matches;
            }
            catch (Exception ex)
            {
                var message = $"Get Operation Failed: {ex.Message}";
                _logger.LogError(ex, message);
                throw new InvalidOperationException(message, ex);
            }
        }

        public async Task<RegistrationResult> InsertWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            user = NormalizeKey(user);

            try
            {
                var registration = ConvertFromWebHook(user, webHook);
                _context.Set<Registration>().Attach(registration);
                _context.Entry(registration).State = EntityState.Added;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException concurrencyEx)
            {
                _logger.LogError(concurrencyEx, concurrencyEx.Message);
                return RegistrationResult.Conflict;
            }
            catch (DbUpdateException updateEx)
            {
                var error = updateEx.GetBaseException().Message;
                _logger.LogError(updateEx, error);
                return RegistrationResult.Conflict;
            }
            catch (SqliteException sqliteEx)
            {
                _logger.LogError(sqliteEx, sqliteEx.Message);
                return RegistrationResult.OperationError;
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, dbEx.Message);
                return RegistrationResult.OperationError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return RegistrationResult.InternalError;
            }
            return RegistrationResult.Success;
        }

        public async Task<RegistrationResult> UpdateWebHookAsync(string user, WebHook webHook)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            user = NormalizeKey(user);

            try
            {

                var registration = await _context.Set<Registration>().Where(r => r.User == user && r.Id == webHook.Id)
                    .FirstOrDefaultAsync();
                if (registration == null)
                {
                    return RegistrationResult.NotFound;
                }

                UpdateRegistrationFromWebHook(user, webHook, registration);
                _context.Entry(registration).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException concurrencyEx)
            {
                _logger.LogError(concurrencyEx, concurrencyEx.Message);
                return RegistrationResult.Conflict;
            }
            catch (SqliteException sqliteEx)
            {
                _logger.LogError(sqliteEx, sqliteEx.Message);
                return RegistrationResult.OperationError;
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, dbEx.Message);
                return RegistrationResult.OperationError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return RegistrationResult.InternalError;
            }
            return RegistrationResult.Success;

        }

        public async Task<RegistrationResult> DeleteWebHookAsync(string user, string id)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            user = NormalizeKey(user);

            try
            {
                var match = await _context.Set<Registration>().Where(r => r.User == user && r.Id == id)
                    .FirstOrDefaultAsync();
                if (match == null)
                {
                    return RegistrationResult.NotFound;
                }

                _context.Entry(match).State = EntityState.Deleted;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException concurrencyEx)
            {
                _logger.LogError(concurrencyEx, concurrencyEx.Message);
                return RegistrationResult.Conflict;
            }
            catch (SqliteException sqliteEx)
            {
                _logger.LogError(sqliteEx, sqliteEx.Message);
                return RegistrationResult.OperationError;
            }
            catch (DbException dbEx)
            {
                _logger.LogError(dbEx, dbEx.Message);
                return RegistrationResult.OperationError;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return RegistrationResult.InternalError;
            }
            return RegistrationResult.Success;
        }

        public async Task DeleteAllWebHooksAsync(string user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            user = NormalizeKey(user);

            try
            {
                var matches = await _context.Set<Registration>().Where(r => r.User == user).ToArrayAsync();
                foreach (var m in matches)
                {
                    _context.Entry(m).State = EntityState.Deleted;
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new InvalidOperationException(ex.Message, ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private string NormalizeKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return key.ToLowerInvariant();
        }

        private WebHook? ConvertToWebHook(Registration? registration)
        {
            if (registration == null)
            {
                return null;
            }

            if (registration.Data == null)
            {
                return null;
            }

            try
            {
                var webHook = JsonSerializer.Deserialize<WebHook>(registration.Data);
                return webHook;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return null;
        }

        private Registration ConvertFromWebHook(string user, WebHook webHook)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }

            var data = JsonSerializer.Serialize(webHook);
            var registration = new Registration
            {
                User = user,
                Id = webHook.Id,
                Data = data
            };
            return registration;
        }

        private void UpdateRegistrationFromWebHook(string user, WebHook webHook, Registration registration)
        {
            if (webHook == null)
            {
                throw new ArgumentNullException(nameof(webHook));
            }
            if (registration == null)
            {
                throw new ArgumentNullException(nameof(registration));
            }

            registration.User = user;
            registration.Id = webHook.Id;

            var data = JsonSerializer.Serialize(webHook);
            registration.Data = data;
        }
    }
}
