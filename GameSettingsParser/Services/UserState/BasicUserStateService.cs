using System.IO;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace GameSettingsParser.Services.UserState
{
    public class BasicUserStateService : IUserStateService
    {
        public string GetUserState(string clientId)
        {
            var userId = GetUserId();
            var input = $"{userId}:{clientId}:{DateTime.UtcNow:yyyy-MM-dd}";
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hash);
            }
        }
        
        private static string GetUserId()
        {
            if (IsDomainJoined() && WindowsIdentity.GetCurrent() != null && WindowsIdentity.GetCurrent().User != null && WindowsIdentity.GetCurrent().User?.Value != null)
            {
                return WindowsIdentity.GetCurrent().User!.Value;
            }
            
            return GetOrCreatePersistentUserId();
        }
    
        private static bool IsDomainJoined()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                return identity.User?.AccountDomainSid != null;
            }
            catch
            {
                return false;
            }
        }

        private static string GetOrCreatePersistentUserId()
        {
            var appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name ?? "GameSettingsParser";
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);
    
            Directory.CreateDirectory(appDataPath);
            var idFile = Path.Combine(appDataPath, "user_id.txt");
    
            if (File.Exists(idFile))
            {
                try
                {
                    var userId = File.ReadAllText(idFile).Trim();
                    
                    if (!string.IsNullOrEmpty(userId))
                        return userId;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading user ID: {ex.Message}");
                }
            }
    
            var newUserId = Guid.NewGuid().ToString("N");
            
            try
            {
                File.WriteAllText(idFile, newUserId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error writing user ID: {ex.Message}");
            }
    
            return newUserId;
        }

    }
}