﻿namespace eFormAPI.Common.Models.User
{
    public class UserRegisterModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }   
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfimation { get; set; }
        public string Role { get; set; }
        public int? RoleId { get; set; }
    }
}
