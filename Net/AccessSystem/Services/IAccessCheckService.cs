using AccessCheckSystem.Models;

namespace AccessCheckSystem.Services
{
    /// <summary>
    /// Интерфейс для сервиса проверки доступа пользователя к входу (выходу) в помещение
    /// </summary>
    interface IAccessCheckService
    {
        /// <summary>
        /// Проверить возможность входа пользователя в помещение или выхода из помещения
        /// </summary>
        /// <param name="roomId">Идентификатор помещения (комнаты)</param>
        /// <param name="entrance">Указание на вход в помещение или выход из помещения (true - вход, false - выход)</param>
        /// <param name="keyId">Идентификатор ключа пользователя</param>
        /// <returns>Перечисление с результатом проверки доступа</returns>
        AccessCheckResult Check(int roomId, bool entrance, int keyId);
    }
}
