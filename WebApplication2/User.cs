﻿namespace WebApplication2
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int Coins { get; set; }

        // Навигационное свойство для связи с UserSkins
        public ICollection<UserSkin> UserSkins { get; set; }
    }
}