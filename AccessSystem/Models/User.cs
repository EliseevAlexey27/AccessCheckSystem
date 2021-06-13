using System.ComponentModel.DataAnnotations;

namespace AccessCheckSystem.Models
{
    /// <summary>
    /// Пользователь
    /// </summary>
    public class User
    {
        /// <summary>
        /// Идентификатор ключа (д.б. уникальный от 1 до 10000)
        /// </summary>
        [Key]
        [Range(1, 10000)]
        public int KeyId { get; set; }

        public User(int keyId)
        {
            KeyId = keyId;
        }
    }
}