using System;
using System.Collections.Generic;

namespace Belpost.Auth
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string PasswordSalt { get; set; } = "";
        public DateTime PasswordChangedAt { get; set; }
        public int FailedAttempts { get; set; } = 0;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockUntil { get; set; }

    
        public List<PasswordHistory> PasswordHistories { get; set; } = new();
    }

    public class PasswordHistory
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string PasswordHash { get; set; } = "";

        public DateTime ChangedAt { get; set; }

        public User? User { get; set; }
    }
}
