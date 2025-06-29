# NEVA Chat API

REST API для чат-приложения NEVA с функциями друзей, постов, комментариев и обмена сообщениями.

## 🚀 Быстрый старт

### Требования
- .NET 9.0
- PostgreSQL
- Docker (опционально)

### Установка и запуск

1. Клонируйте репозиторий:
```bash
git clone <repository-url>
cd NEVA.FriendsSystem/Chat
```

2. Настройте строку подключения к базе данных в `External/WebApi/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=neva_chat;Username=postgres;Password=postgres"
  }
}
```

3. Запустите приложение:
```bash
cd External/WebApi
dotnet run
```

4. Откройте Swagger UI: `https://localhost:5001/swagger`

## 📚 API Документация

### Swagger UI
Полная документация API доступна через Swagger UI по адресу `/swagger` после запуска приложения.

### Основные эндпоинты

#### Профиль пользователя
- `GET /api/profile/{userId}` - Получить профиль пользователя
- `PUT /api/profile/` - Обновить профиль пользователя
- `POST /api/profile/validate-username` - Проверить доступность имени пользователя

#### Блог
- `POST /api/blog/` - Создать новый пост
- `GET /api/blog/user/{userId}/posts` - Получить посты пользователя
- `GET /api/blog/posts/{postId}/comments` - Получить комментарии к посту
- `POST /api/blog/posts/{postId}/comments` - Добавить комментарий к посту

#### Друзья
- `POST /api/friends/` - Добавить друга
- `GET /api/friends/` - Получить список друзей
- `DELETE /api/friends/` - Удалить друга
- `POST /api/friends/blacklist/` - Заблокировать пользователя

#### Чаты
- `GET /api/users/chats/page={page}/size={size}` - Получить чаты пользователя

#### Медиа
- `GET /api/media/stickers/page={page}/size={size}` - Получить стикеры
- `GET /api/media/reactions/page={page}/size={size}` - Получить реакции
- `POST /api/media/stickers/` - Создать стикер
- `POST /api/media/reactions/` - Создать реакцию

### Аутентификация
API использует JWT токены для аутентификации. Добавьте заголовок:
```
Authorization: Bearer <your-jwt-token>
```

### Пагинация
Большинство эндпоинтов поддерживают пагинацию через параметры:
- `page` - номер страницы (начиная с 1)
- `size` - размер страницы

### Загрузка файлов
Для загрузки файлов используйте `multipart/form-data`:
- Аватары и обложки профиля
- Изображения к постам и комментариям
- Стикеры и реакции

## 🏗️ Архитектура

Проект использует Clean Architecture с разделением на слои:

- **Application** - Бизнес-логика и use cases
- **Domain** - Модели домена и интерфейсы
- **Infrastructure** - Реализация репозиториев и внешних сервисов
- **WebApi** - REST API контроллеры и конфигурация

### Основные технологии
- ASP.NET Core 9.0
- Entity Framework Core
- PostgreSQL
- FluentValidation
- Swashbuckle (Swagger)
- MediatR (CQRS)

## 🔧 Разработка

### Добавление новых эндпоинтов
1. Создайте запрос/команду в `Application/Requests/`
2. Реализуйте обработчик
3. Добавьте эндпоинт в соответствующий файл в `WebApi/Endpoints/`
4. Добавьте Swagger аннотации для документации

### Swagger аннотации
Используйте следующие атрибуты для документирования API:

```csharp
[SwaggerOperation(
    Summary = "Краткое описание",
    Description = "Подробное описание",
    OperationId = "UniqueOperationId",
    Tags = new[] { "TagName" }
)]
[SwaggerResponse(200, "Успешный ответ")]
[SwaggerResponse(400, "Ошибка валидации")]
```

## 📝 Лицензия

MIT License - см. файл LICENSE для подробностей.

## 🤝 Поддержка

По вопросам и предложениям обращайтесь:
- Email: support@neva-chat.com
- Website: https://neva-chat.com 