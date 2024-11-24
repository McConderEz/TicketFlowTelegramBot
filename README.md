# TicketFlowTelegramBot

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)

Телеграм-бот для удобной работы с талонами почты и бронированием.

## Возможности:
- [x] Аутентификация пользователей
- [x] Оставление отзывов
- [x] Бронирование талонов
- [x] Получение справочной информации
- [x] Получение состояния очереди
- [x] Получение контактов поддержки

## Стек:
- C# — язык программирования
- [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) ^18.0.0 — библиотека для работы с Telegram Bot API
- [PRTelegramBotClient](https://github.com/ProgrammingRussia/PRTelegramBotClient) ^1.8.0 — библиотека для работы с Telegram Bot API
- Entity Framework Core 8 — ORM для работы с базой данных
- SQLite — база данных

## Установка и запуск

### Посредством Docker

1. Установите и настройте [Docker](https://www.docker.com/), как указано в документации.
2. Из папки проекта выполните команду:
```shell
docker build -t ticketflow-telegram-bot .
```
3. Теперь запускать бот можно командой:
```shell
docker run -it -d ticketflow-telegram-bot
```

### Без использования Docker

1. Убедитесь, что у вас установлен [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
2. Создайте файл `appsettings.json` на основе `appsettings.template.json` и присвойте переменным соответствующие значения.
3. Выполните миграции базы данных:
```shell
dotnet ef database update
```
4. Теперь запускать бота можно командой:
```shell
dotnet run --project src/TicketFlowTelegramBot
```

## Конфигурация

Основные настраиваемые параметры:

- Токен Telegram Bot API
- Настройки подключения к базе данных (SQLite)
- Настройки аутентификации
- Настройки отзывов
- Настройки бронирования талонов
- Настройки справочной информации
- Настройки состояния очереди
- Настройки контактов поддержки

Все параметры конфигурации находятся в файле `appsettings.json`.
