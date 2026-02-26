# Z-media Army Clash (Unity, мобильный прототип)

## 1. О проекте

Тестовое задание: реализовать базовый симулятор боя в духе Army Clash.

Что сделано:
- Две армии по 20 юнитов, сражаются до полного уничтожения одной из сторон.
- Перед боем можно рандомизировать левую, правую или обе армии.
- У каждого юнита есть `HP`, `ATK`, `SPEED`, `ATKSPD`.
- Характеристики зависят от `Shape`, `Size`, `Color`.
- Реализована дополнительная фича: **God's Wrath** (заряд за убийства, drag-and-drop каст по арене, AoE с friendly fire).

Платформа: мобильная (Android/iOS), проект выполнен в Unity.

## 2. Версия Unity и зависимости

- Unity: `6000.3.6f1` (см. `ProjectSettings/ProjectVersion.txt`)
- Основные пакеты:
  - `com.unity.inputsystem` `1.18.0`
  - `com.unity.test-framework` `1.6.0`
  - `com.gustavopsantos.reflex` `14.2.0` (DI)
  - `com.cysharp.r3` `1.3.0` (reactive-потоки)
  - `com.github-glitchenzo.nugetforunity` `v4.5.0`
- DOTween подключен в проекте как плагин (`Assets/Plugins/Demigiant/DOTween`).

## 3. Архитектурный подход и мотивация

Выбран слоистый подход с разделением ответственности:

- `Domain` (чистая бизнес-логика)
- `Application` (use cases, orchestration боевого цикла)
- `Infrastructure` (конфиги, адаптеры, DI wiring)
- `Presentation` (Unity MonoBehaviour, UI Toolkit, VFX, binding, input)
- Внутри слоёв используется feature-first группировка (после реорганизации папок).

Почему так:
- Упрощает расширение и поддержку в долгую.
- Позволяет тестировать ядро отдельно от Unity-сцены.
- Удобно добавлять новые формы/цвета/механики без переписывания всего проекта.

Используемые паттерны:
- `MVVM` для UI (ViewModel на `R3`)
- `State Machine` для фаз боя
- `Dependency Injection` через `Reflex`
- `Reactive`-коммуникация вместо runtime C# events для бизнес-потоков

## 4. Структура проекта (после реорганизации)

```text
Assets/ZMediaTask
  Domain/
    Army/
    Traits/
    Random/
    Combat/
      Common/
      Attack/
      Movement/
      Navigation/
      Formation/
      Knockback/
      Wrath/
  Application/
    Army/
    Battle/
      Setup/
      State/
      Events/
      Loop/
      Wrath/
  Infrastructure/
    DI/
      Installers/
    Random/
      Providers/
    Config/
      Combat/
        Arena/
        Formation/
        Gameplay/
      Traits/
        Catalog/
        Weights/
  Presentation/
    Input/
      Abstractions/
      Adapters/
    Presenters/
      Core/
      Battle/
      Wrath/
      Layout/
    Services/
      Battle/
      Layout/
      Vfx/
    ViewModels/
      Flow/
      Battle/
      Wrath/
    UI/
    Prefabs/
    Shaders/
  Tests/
    EditMode/
    PlayMode/
Assets/Scenes/GameScene.unity
```

Ключевые конфиги:
- Трейты: `Assets/ZMediaTask/Infrastructure/Config/Traits/UnitTraitCatalog.asset`
- Веса рандома: `Assets/ZMediaTask/Infrastructure/Config/Traits/UnitTraitWeightCatalog.asset`
- Боевые параметры: `Assets/ZMediaTask/Infrastructure/Config/Combat/CombatGameplayConfig.asset`
- Формации: `Assets/ZMediaTask/Infrastructure/Config/Combat/FormationConfig.asset`

## 5. Модель юнита и формулы

Базовые статы:
- `HP = 100`
- `ATK = 10`
- `SPEED = 10`
- `ATKSPD = 1`

Модификаторы:

**Shape**
- `Cube`: `+100 HP`, `+10 ATK`
- `Sphere`: `+50 HP`, `+20 ATK`

**Size**
- `Small`: `-50 HP`
- `Big`: `+50 HP`

**Color**
- `Blue`: `-15 ATK`, `+4 ATKSPD`, `+10 SPEED`
- `Green`: `-100 HP`, `+20 ATK`, `-5 SPEED`
- `Red`: `+200 HP`, `+40 ATK`, `-9 SPEED`

Расчёт:
- Итоговые статы = `BaseStats + ShapeMod + SizeMod + ColorMod`
- Минимум по каждому стату ограничен нулем.

Масштабируемость:
- Нет жёстких `switch` в бизнес-логике для trait-формул.
- Используются каталоги модификаторов/весов на `ScriptableObject` + валидаторы.

## 6. Логика симуляции

Фазы боя:
- `Preparation`
- `Running`
- `Finished`

Цикл симуляции:
- Фиксированный тик (`0.02s`, настраивается в `CombatGameplayConfig`).
- На каждом тике:
  - выбор цели (ближайший живой противник),
  - движение к целям (ближний бой),
  - локальное разведение юнитов (steering, slot allocation),
  - атаки при входе в радиус,
  - обработка смертей и победителя.

Правило атаки:
- Чем выше `ATKSPD`, тем медленнее атака.
- Формула cooldown: `nextAttackTime = now + baseAttackDelay * ATKSPD`.

## 7. Дополнительная фича: God's Wrath

Реализовано:
- Заряд Wrath получает только сторона игрока (левая армия) за убийства врагов.
- При полном заряде можно сделать drag карты скилла по экрану.
- При отпускании над ареной создается AoE-каст с задержкой импакта.
- Урон получают все юниты в радиусе (включая союзников, то есть friendly fire).
- Невалидная точка каста отменяет применение без траты заряда.
- Параметры вынесены в `CombatGameplayConfig.asset` (текущее значение по урону в проекте: `800`).

UI/UX:
- Карта Wrath в UI Toolkit (`WrathCard`).
- В режиме таргетинга включается визуальный индикатор области.
- Для читаемости момента таргетинга используется локальный slow-motion.

## 8. Производительность и поддерживаемость

Что сделано для стабильности:
- Разделение чистой логики и Unity-представления.
- Настраиваемые параметры через `ScriptableObject` (баланс без правки кода).
- Простой и предсказуемый fixed-step loop.
- Пул VFX (`VfxPool`) для снижения аллокаций во время боя.

Ограничения текущей версии:
- Pathfinding базовый (`DirectPathfinder`) для плоской арены без препятствий.
- Одна игровая сцена (`GameScene`) и один боевой режим.

## 9. Тесты

Тесты находятся в:
- `Assets/ZMediaTask/Tests/EditMode`
- `Assets/ZMediaTask/Tests/PlayMode`

Покрываются:
- расчёт статов, бой, навигация, formation-стратегии, Wrath-логика, ViewModel, интеграция боевого цикла.

Текущий объём тестов (по атрибутам `[Test]/[UnityTest]`):
- EditMode: `149`
- PlayMode: `5`
- Всего: `154`

Запуск:
- Через Unity Test Runner (рекомендуется для локальной проверки в редакторе).
- Либо через Unity MCP/CLI в CI.

## 10. Как запустить

1. Открыть проект в Unity Hub.
2. Убедиться, что используется Unity `6000.3.6f1`.
3. Открыть сцену `Assets/Scenes/GameScene.unity`.
4. Нажать Play.

Игровой flow:
- Main Menu -> Preparation (randomize) -> Start Battle -> Result -> Return.

## 11. Принятые ключевые решения

- Упор на расширяемость: новые trait-правила/веса и боевые параметры задаются данными.
- Упор на тестируемость: основная логика вынесена в `Domain/Application`.
- Упор на управляемость UI-состояний: реактивные ViewModel и явные переходы по состояниям.

## 12. Время выполнения ТЗ

- Потраченное время: `14 часов`.
