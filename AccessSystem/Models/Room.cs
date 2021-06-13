using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AccessCheckSystem.Models
{
    /// <summary>
    /// Помещение (комната)
    /// </summary>
    public class Room
    {
        /// <summary>
        /// Идентификтор помещения (д.б. уникальный от 1 до 5)
        /// </summary>
        [Key] 
        [Range(1, 5)] 
        public int RoomId { get; set; }

        /// <summary>
        /// Пользователи, находящиеся в помещении
        /// </summary>
        public List<User> Users { get; set; }

        public Room(int roomId)
        {
            RoomId = roomId;
            Users = new List<User>();
        }
    }


}