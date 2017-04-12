using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class UserPassword : IModelBuilderExtender
    {
        [Key]
        public int UserId { get; set; }

        [MinLength(4, ErrorMessage = "Please enter a password (minimum lenght = 4).")]
        [Required(AllowEmptyStrings = false,
                  ErrorMessage = "Please enter a password (minimum lenght = 4)."
                  )
        ]
        public string Password { get; set; }

        [ForeignKey("UserId")]
        public virtual OsbideUser User { get; set; }

        /// <summary>
        /// Encrypts the supplied password for the given user
        /// </summary>
        /// <param name="password"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static string EncryptPassword(string password, OsbideUser user)
        {
            return EncryptPassword(password, user.Email);
        }

        /// <summary>
        /// Encryptps the supplied password using the supplied salt.  By default, salts should be
        /// the user's email address
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string EncryptPassword(string password, string salt)
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
            byte[] saltedHashBytes = GenerateSaltedHash(passwordBytes, saltBytes);
            return Convert.ToBase64String(saltedHashBytes);
        }

        /// <summary>
        /// Validates the supplied user/pass combo.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>True if the combo is valid, false otherwise</returns>
        public static bool ValidateUser(string email, string password, OsbideContext db)
        {
            string hashedPassword = UserPassword.EncryptPassword(password, email);
            return ValidateUserHashedPassword(email, hashedPassword, db);
        }

        /// <summary>
        /// Validates the supplied user/pass combo.  Unlike ValidateUser, this function assumes a pre-hashed password.
        /// Useful for not sending a naked password over the wire.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="hashedPassword"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool ValidateUserHashedPassword(string email, string hashedPassword, OsbideContext db)
        {
            int count = 0;
            count = (from user in db.Users
                     join pword in db.UserPasswords on user.Id equals pword.UserId
                     where user.Email.CompareTo(email) == 0
                     &&
                     pword.Password.CompareTo(hashedPassword) == 0
                     select user
                      ).Count();
            if (count == 1)
            {
                return true;
            }
            return false;
        }

        private static byte[] GenerateSaltedHash(byte[] plainText, byte[] salt)
        {
            //TODO: Upgrade to SHA512 in next version release
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] plainTextWithSaltBytes =
              new byte[plainText.Length + salt.Length];

            for (int i = 0; i < plainText.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainText[i];
            }
            for (int i = 0; i < salt.Length; i++)
            {
                plainTextWithSaltBytes[plainText.Length + i] = salt[i];
            }
            return algorithm.ComputeHash(plainTextWithSaltBytes);
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserPassword>()
                .HasRequired(up => up.User)
                .WithMany()
                .WillCascadeOnDelete(true);
        }
    }
}
