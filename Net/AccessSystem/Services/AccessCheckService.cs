using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using AccessCheckSystem.Models;
using Newtonsoft.Json;
using NLog;

namespace AccessCheckSystem.Services
{
    /// <summary>
    /// Сервис пропускной системы
    /// Выполняет основную логику доступа пользователей в помещения (комнаты)
    /// </summary>
    public class AccessCheckService: IAccessCheckService
    {
        /// <summary>
        /// Путь к текстовому файлу, в котором хранится состояния помещений
        /// (какие пользователи в каких подразделениях находятся)
        /// Путь к файлу складывается из каталога приложения и относительного пути, прописанного в Web.config (appSettings->RoomsFilePath)
        /// </summary>
        private readonly string _roomsFilePath = $@"{AppDomain.CurrentDomain.BaseDirectory}\{ConfigurationManager.AppSettings["RoomsFilePath"]}";

        /// <summary>
        /// Логгирование
        /// </summary>
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Состояние помещений (какие пользователи в каких помещениях находятся)
        /// </summary>
        private readonly List<Room> _usersRooms;

        /// <summary>
        /// Конструктор сервиса
        /// </summary>
        public AccessCheckService()
        {
            _usersRooms = ReadData() ?? new List<Room>();
        }

        /// <summary>
        /// Получение данных по помещениям, в которых находятся пользователи, из текстового файла
        /// </summary>        
        private List<Room> ReadData()
        {
            _logger.Trace("Чтение данных начато");

            string data = File.ReadAllText(_roomsFilePath);
            List<Room> result = JsonConvert.DeserializeObject<List<Room>>(data) ?? new List<Room>();

            _logger.Trace("Чтение данных завершено");

            return result;
        }

        /// <summary>
        /// Сохранение данных по помещениям, в которых находятся пользователи, в текстовый файл
        /// </summary>
        private void SaveChanges()
        {
            _logger.Trace("Сохранение данных начато");

            File.WriteAllText(_roomsFilePath, JsonConvert.SerializeObject(_usersRooms));

            _logger.Trace("Сохранение данных завершено");
        }        

        /// <summary>
        /// Добавление ключа пользователя в помещение
        /// </summary>
        /// <param name="roomId">Идентификтор помещения</param>
        /// <param name="keyId">Идентификтор ключа пользователя</param>
        private void AddUserKeyToRoom(int roomId, int keyId)
        {
            // Получаем помещение, если помещения нет, то добавим в общий контейнер помещений
            Room room = _usersRooms.SingleOrDefault(r => r.RoomId == roomId);
            if (room == null)
            {
                room = new Room(roomId);
                _usersRooms.Add(room);
            }

            // На всякий случай проверим добавлен ли ключ пользователя в помещение
            // Если ключа еще нет в помещении, то добавим
            if (!room.Users.Exists(u => u.KeyId == keyId))
            {
                room.Users.Add(new User(keyId));
            }
        }

        /// <summary>
        /// Удаление ключа пользователя из помещения
        /// </summary>
        /// <param name="roomId">Идентификатор помещения</param>
        /// <param name="keyId">Идентификатор ключа</param>
        private void DeleteUserKeyFromRoom(int roomId, int keyId)
        {
            Room room = _usersRooms.SingleOrDefault(r => r.RoomId == roomId);
            if (room != null && room.Users.Exists(u => u.KeyId == keyId))
            {
                room.Users.RemoveAll(u => u.KeyId == keyId);
            }
        }

        /// <summary>
        /// Проверка на валидность входящих данных по помещению и ключу
        /// </summary>
        /// <param name="roomId">Идентификтор помещения</param>
        /// <param name="keyId">Идентификтор ключа</param>
        /// <returns>Валидны ли входящиее данные (true - валидны, false - не валидны)</returns>
        private bool IncomingDataIsValid(int roomId, int keyId)
        {
            // Комнаты должны быть только от 1 до 5 вкл
            if (roomId < 1 || roomId > 5)
            {
                _logger.Error($"Идентификатор комнаты '{roomId}' не входит в диапазон [1,5]");
                return false;
            }

            if (keyId < 1 || keyId > 10000)
            {
                _logger.Error($"Идентификатор ключа пользователя '{keyId}' не входит в диапазон [1,10000]");
                return false;
            }

            // Пользователю можно входить только в те помещения, на номер которого делится его ид.
            if (keyId % roomId != 0)
            {
                _logger.Error($"Номер ключа '{keyId}' не делится на номер помещения '{roomId}'");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Реализация метода Check (проверки доступа) для интерфейса IAccessCheckService
        /// </summary>
        /// <param name="roomId">Идентификатор помещения (комнаты)</param>
        /// <param name="entrance">Указание на вход в помещение или выход из помещения (true - вход, false - выход)</param>
        /// <param name="keyId">Идентификатор ключа пользователя</param>
        /// <returns>Перечисление с результатом проверки доступа</returns>
        public AccessCheckResult Check(int roomId, bool entrance, int keyId)
        {            
            AccessCheckResult result;
            try
            {
                string entranceAsString = entrance ? "вход" : "выход";
                _logger.Info($"Попытка доступа. Помещение: {roomId}, Ключ: {keyId}, Операция: {entranceAsString}");

                // Если входящие данные по комнате и ключу не валидны, то доступ запрещен
                if (!IncomingDataIsValid(roomId, keyId))
                {
                    _logger.Error("Данные по комнате и ключу не валидны. Доступ запрещен.");
                    return AccessCheckResult.Denied;
                }

                // Получим помещение, в котором сейчас находится пользователь
                Room userRoom = _usersRooms.FirstOrDefault(r => r.Users.Exists(u => u.KeyId == keyId));

                _logger.Trace(userRoom != null
                    ? $"Пользователь с ключом '{keyId}' находится в помещении '{userRoom.RoomId}'"
                    : $"Пользователь с ключом '{keyId}' не находится ни в каком помещении");

                // Если пользователь сейчас находится в каком-то помещении
                if (userRoom != null)
                {
                    if (entrance)
                    {
                        _logger.Info($"Пользователю с ключом '{keyId}' нельзя войти в помещение '{roomId}', поскольку он сейчас находится в помещении '{userRoom.RoomId}'. Доступ запрещен");
                        return AccessCheckResult.Denied;
                    }
                    // not entrance
                    if (userRoom.RoomId != roomId)
                    {
                        _logger.Info($"Пользователю с ключом '{keyId}' нельзя выйти из помещения '{roomId}', поскольку он сейчас находится в другом помещении '{userRoom.RoomId}'. Доступ запрещен");
                        return AccessCheckResult.Denied;
                    }
                }
                // Пользователь не находится ни в каком помещении и доступ на выход
                else if (!entrance)
                {
                    _logger.Info(
                        $"Пользователь с ключом '{keyId}' не находится ни в каком помещении, поэтому выйти из комнаты '{roomId}' не может. Доступ запрещен");
                    return AccessCheckResult.Denied;
                }
                
                // Если доступ на вход, то добавим ключ пользователя в комнату
                if (entrance)
                {
                    AddUserKeyToRoom(roomId, keyId);
                }
                // Если доступ на выход, то удалим ключ пользователя из комнаты
                else
                {
                    DeleteUserKeyFromRoom(roomId, keyId);
                }

                SaveChanges();

                result = AccessCheckResult.Allowed;
                _logger.Info(
                    $"Пользователю с ключом '{keyId}' РАЗРЕШЕН {entranceAsString} в (из) помещение(я) '{roomId}'");

            }
            catch (Exception ex)
            {
                _logger?.Error(ex);
                result = AccessCheckResult.Error;
            }               

            return result;
        }
        
    }
}