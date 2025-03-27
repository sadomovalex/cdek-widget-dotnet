# cdek-widget-dotnet
Для запуска потребуется:

1. CDEK API ClientId и ClientSecret. Для тестового аккаунта CDEK их можно найти здесь:
https://api-docs.cdek.ru/29923849.html > раздел Подключение к сервису

2. Ключ для API Яндекс Карт: необходимо сгенерировать в кабинете разработчика:
https://developer.tech.yandex.ru/services/3
и в настройки ключа в поле "Ограничение по HTTP Referer" добавить доменное имя cdek.example.ru.
Обратить внимание: применение настроек ограничения по HTTP Referer для ключа Яндекс Карт требует некоторого времени.

Данные настройки необходимо указать в appsettings.json.
Проект использует тестовый endpoint CDEK API https://api.edu.cdek.ru/v2 (также указан в appsettings.json).

3. Перенаправить запросы http://cdek.example.ru на localhost.
Напр. для Windows для этого надо добавить запись в файл C:\Windows\System32\drivers\etc\hosts:
```
127.0.0.1	cdek.example.ru
```

При запуске проигнорировать предупреждение браузера о небезопасном https соединении.