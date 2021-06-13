using System;
using System.Net;
using System.Web.Http;
using AccessCheckSystem.Models;
using AccessCheckSystem.Services;
using NLog;

namespace AccessCheckSystem.Controllers
{
    /// <summary>
    /// Контроллер для пропускной системы
    /// </summary>
    public class AccessCheckSystemController : ApiController
    {
        /// <summary>
        /// Логгирование
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Сервис пропусной системы
        /// </summary>
        private readonly IAccessCheckService _accessCheckService = new AccessCheckService();
        
        /// <summary>
        /// Проверка возможности входа пользователя в помещение или выхода из помещения
        /// </summary>
        /// <param name="roomId">Идентификтор помещения (комнаты)</param>
        /// <param name="entrance">Указание на вход в помещение или выход из помещения (true - вход, false - выход)</param>
        /// <param name="keyId">Идентификатор ключа пользователя</param>
        /// <returns>
        /// 200 - дверь можно открыть
        /// 403 - запрет на вход
        /// 500 - ошибка
        /// </returns>
        [HttpGet]
        public IHttpActionResult Check(int roomId, bool entrance, int keyId)
        {
            try
            {
                AccessCheckResult checkResult = _accessCheckService.Check(roomId, entrance, keyId);

                _logger.Info($"Результат проверки доступа: {checkResult}");

                switch (checkResult)
                {
                    case AccessCheckResult.Allowed:
                    {
                        // 200 - OK
                        return Ok();
                    }
                    case AccessCheckResult.Denied:
                    {
                        // 403 - запрет на вход
                       return StatusCode(HttpStatusCode.Forbidden);                            
                    }
                    default:
                    {
                        // 500 - ошибка
                        return InternalServerError();
                    }                        
                }                                               
            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                // 500 - Ошибка
                return InternalServerError(ex);
            }            
        }
    }
}
