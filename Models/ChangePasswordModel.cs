﻿namespace OLA.Models
{
    public class ChangePasswordModel
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
