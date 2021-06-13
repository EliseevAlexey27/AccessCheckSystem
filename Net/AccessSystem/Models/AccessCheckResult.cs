namespace AccessCheckSystem.Models
{    
    /// <summary>
    /// Результат проверки доступа
    /// </summary>
    public enum AccessCheckResult
    {
        /// <summary>
        /// Разрешен
        /// </summary>
        Allowed = 0,

        /// <summary>
        /// Запрещен
        /// </summary>
        Denied = 1,

        /// <summary>
        /// Ошибка
        /// </summary>
        Error = 2
    }
}