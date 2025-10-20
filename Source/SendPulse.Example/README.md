# SendPulse.Example

Примеры использования SendPulse .NET Client.

## Настройка

### 1. Создайте файл конфигурации

Скопируйте файл `appsettings.json.example` в `appsettings.json`:

```bash
copy appsettings.json.example appsettings.json
```

или на Linux/Mac:

```bash
cp appsettings.json.example appsettings.json
```

### 2. Добавьте свои учетные данные

Откройте `appsettings.json` и замените значения на ваши реальные учетные данные SendPulse:

```json
{
  "SendPulse": {
    "ClientId": "your-actual-client-id",
    "ClientSecret": "your-actual-client-secret",
    "BaseUrl": "https://api.sendpulse.com"
  }
}
```

### 3. Получение учетных данных

Получить `ClientId` и `ClientSecret` можно в личном кабинете SendPulse:
1. Перейдите в [SendPulse](https://sendpulse.com)
2. Войдите в аккаунт
3. Перейдите в раздел "Настройки" → "API"
4. Создайте новое приложение или используйте существующее

## Запуск

```bash
dotnet run
```

## Безопасность

⚠️ **Важно**: Файл `appsettings.json` добавлен в `.gitignore` и не должен коммититься в репозиторий, так как содержит секретные данные!

Всегда используйте `appsettings.json.example` как образец для других разработчиков.

