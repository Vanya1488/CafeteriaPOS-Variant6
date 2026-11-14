# Результаты интеграционного тестирования CafeteriaPOS

**МДК 03.02 — вариант №6**

**Студент:** Исаев Иван Андреевич  
**Группа:** И13-24-11  
**Проект:** CafeteriaPOS  
**Язык:** C#  
**Платформа:** .NET 8  
**Инструменты:** xUnit, FluentAssertions, WebApplicationFactory  
**Дата итогового тестирования:** 15.11.2026

## 1. Цель тестирования

Проверить корректность работы REST API CafeteriaPOS, сохранение функциональности после рефакторинга и эффективность реализованных механизмов защиты административных операций.

## 2. Проверенные тесты

### 2.1. Основные интеграционные тесты

| № | Название теста | Проверяемое поведение | Результат |
|---|---|---|---|
| 1 | GetMenu_ReturnsThreeItems | Получение трёх позиций меню — HTTP 200 | PASS |
| 2 | GetOrders_ReturnsEmptyList | Получение пустого списка заказов — HTTP 200 | PASS |
| 3 | CreateOrder_ValidRequest_Returns201 | Создание заказа, сумма 490 — HTTP 201 | PASS |
| 4 | CreateOrder_EmptyItems_Returns400 | Отклонение пустого заказа — HTTP 400 | PASS |
| 5 | CreateOrder_UnavailableItem_Returns400 | Недоступное блюдо — HTTP 400 | PASS |
| 6 | GetOrderV2_ExistingOrder_Returns200 | Получение заказа через V2 — HTTP 200 | PASS |
| 7 | GetOrderV2_MissingOrder_Returns404 | Отсутствующий заказ — HTTP 404 | PASS |
| 8 | Health_ReturnsHealthy | Проверка /health — HTTP 200 | PASS |

### 2.2. Проверки FluentAssertions

| № | Название теста | Результат |
|---|---|---|
| 9 | HealthEndpoint_ReturnsHealthy_WithFluentAssertions | PASS |
| 10 | MissingOrder_ReturnsNotFound_WithFluentAssertions | PASS |

### 2.3. Тестирование безопасности ПР8

| № | Название теста | Ожидаемый результат | Статус |
|---|---|---|---|
| 11 | PriceUpdate_WithoutKey_Returns401 | Без ключа — HTTP 401 | PASS |
| 12 | PriceUpdate_WrongKey_Returns401 | Неверный ключ — HTTP 401 | PASS |
| 13 | PriceUpdate_CashierKey_Returns403 | Недостаточные права — HTTP 403 | PASS |
| 14 | PriceUpdate_AdminKey_Returns204AndUpdatesMenu | Администратор изменяет цену — HTTP 204 | PASS |
| 15 | PriceUpdate_InvalidPrice_Returns400 | Недопустимая цена — HTTP 400 | PASS |

## 3. Итоговые результаты

| Показатель | Значение |
|---|---:|
| Всего тестов | 15 |
| Успешно пройдено | 15 |
| Неуспешно | 0 |
| Пропущено | 0 |
| Время выполнения тестов | 3,8 секунды |
| Состояние сборки | Успешно |

Результаты подтверждены выполнением dotnet test в Visual Studio 2026.

## 4. Реализованные механизмы защиты

В учебном проекте добавлены следующие механизмы:

1. Проверка демонстрационного ключа доступа к административной операции.
2. Разграничение ролей администратора и кассира.
3. Возврат HTTP 401 для неавторизованных запросов.
4. Возврат HTTP 403 при недостаточных полномочиях.
5. Проверка допустимости стоимости блюда с FluentValidation.
6. Ограничение частоты запросов с помощью Rate Limiting.
7. Локальное хранение секретов с использованием .NET User Secrets.

Автоматизированные проверки непосредственно подтверждают сценарии с кодами HTTP 400, 401, 403 и 204. Отдельное нагрузочное тестирование Rate Limiting в этом наборе не проводилось.

## 5. Запуск тестов

Из корня проекта выполнить:

```powershell
dotnet restore .\CafeteriaPOS.Tests\CafeteriaPOS.Tests.csproj
dotnet build .\CafeteriaPOS.Tests\CafeteriaPOS.Tests.csproj
dotnet test .\CafeteriaPOS.Tests\CafeteriaPOS.Tests.csproj
```

## 6. Исходный код тестов

- [Основные тесты](../CafeteriaPOS.Tests/UnitTest1.cs)
- [FluentAssertions](../CafeteriaPOS.Tests/FluentAssertionsTests.cs)
- [Тесты безопасности](../CafeteriaPOS.Tests/SecurityTests.cs)
- [Настройки тестового проекта](../CafeteriaPOS.Tests/CafeteriaPOS.Tests.csproj)

## 7. Заключение

Выполнено интеграционное тестирование REST API CafeteriaPOS после рефакторинга и реализации дополнительных механизмов безопасности.

Все 15 тестов завершились успешно. Подтверждена работоспособность проверенных функций создания и получения заказов, обработки некорректных запросов, проверки состояния сервиса и разграничения доступа к административной операции.

Реализованные меры повышают безопасность учебного прототипа, но не заменяют полноценный аудит и испытания промышленной системы.

**Репозиторий:** https://github.com/Vanya1488/CafeteriaPOS-Variant6
