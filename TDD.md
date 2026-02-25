# Z-media Army Clash — TDD Plan (No ECS, Reflex, UI Toolkit, Input System, R3)

## 1. Цель и рамки

Сделать поддерживаемый мобильный прототип симулятора боя двух армий по ТЗ:

- 2 армии по 20 юнитов.
- Перед стартом можно рандомизировать армии.
- Автобой до полного уничтожения одной стороны.
- 4 характеристики юнита: `HP`, `ATK`, `SPEED`, `ATKSPD`.
- Характеристики вычисляются из базовых + модификаторы `Shape/Size/Color`.
- Доп. фича: одна, но завершенная по качеству.

Ограничения разработки:

- Без ECS.
- DI через `Reflex`.
- UI через `UI Toolkit`.
- Input через `New Input System`.
- Связь Model/View через `R3` (без C# events в runtime-логике).

---

## 2. Стек и конфигурация проекта

## 2.1 Unity и платформа

- Unity: `6000.3.6f1` (уже в проекте).
- Build target: Android (основной), iOS (проверка компиляции по возможности).
- Render: URP (уже включен).

## 2.2 Пакеты

Уже есть:

- `com.unity.inputsystem` (1.18.0)
- `com.unity.test-framework` (1.6.0)
- `com.unity.ugui` (2.0.0, можно оставить для совместимости)

Добавить:

- Reflex: `com.gustavopsantos.reflex` (git URL, сейчас зафиксирован `14.2.0`).
- NuGetForUnity: `com.github-glitchenzo.nugetforunity` (для установки NuGet DLL в Unity-проект).
- R3:
  - Core `R3.dll` через NuGetForUnity (сейчас установлен `R3 1.3.0` в `Assets/Packages/R3.1.3.0/...`).
  - `R3.Unity` через git URL (Unity-обвязка):
    - `https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity#<tag>`
- DOTween:
  - `DOTween` (Demigiant; через UPM-совместимый пакет или Asset Store импорт, зафиксировать версию в README).

Принципы фиксации зависимостей:

- Пиновать версии (без `latest`).
- Коммитить `Packages/packages-lock.json`.
- Не менять версии в середине фичи без причины.

## 2.3 Player/Input конфиг

- `Active Input Handling`: `Input System Package (New)` или `Both`.
- Создать `InputActions` asset:
  - `UI/Point`, `UI/Click`, `UI/Submit`
  - `Battle/RandomizeLeft`, `Battle/RandomizeRight`, `Battle/RandomizeBoth`, `Battle/Start`.

## 2.4 UI Toolkit конфиг

- Main scene: `UIDocument` + `PanelSettings`.
- Базовые UXML:
  - `MainMenuView.uxml`
  - `PreparationView.uxml`
  - `BattleHudView.uxml`
  - `ResultView.uxml`
- USS:
  - `Common.uss`
  - `Preparation.uss`
  - `Battle.uss`

---

## 3. Архитектурное решение

## 3.1 Слои

1. `Domain` (чистая логика, без Unity API):
   - Расчет статов, сущности юнитов/армий, правила боя.
2. `Application`:
   - Use cases, orchestration симуляции, state machine экрана.
3. `Infrastructure`:
   - Random provider, time/tick provider, scene loading adapter.
4. `Presentation`:
   - ViewModel на `R3`, UI Toolkit binding, input adapters, DOTween animation adapters.

## 3.2 Правила зависимости

- `Presentation -> Application -> Domain`
- `Infrastructure -> Application/Domain` через интерфейсы.
- Domain не знает про Unity/Reflex/UI.

## 3.3 DI (Reflex)

- `ProjectScope`:
  - Config providers, factories, scene services.
- `SceneScope`:
  - Бой: `BattleLoopService`, `BattleViewModel`, input adapters.

Биндинги:

- Domain сервисы как singleton.
- Симуляция/сессия как scoped/transient (на новый бой).

## 3.4 Реактивная коммуникация (R3)

Вместо C# events:

- В VM: `ReactiveProperty<T>`, `ReadOnlyReactiveProperty<T>`, `Subject<T>`.
- Взаимодействие:
  - input stream -> use case
  - battle state stream -> UI updates
- Диспозинг:
  - один `Disposable` контейнер на View/VM.
  - обязательный teardown при смене экрана/сцены.

---

## 4. Доменные модели и формулы

## 4.1 Enums

- `UnitShape`: `Cube`, `Sphere`.
- `UnitSize`: `Small`, `Big`.
- `UnitColor`: `Blue`, `Green`, `Red`.
- `ArmySide`: `Left`, `Right`.

## 4.2 Базовые статы

- `HP = 100`
- `ATK = 10`
- `SPEED = 10`
- `ATKSPD = 1`

## 4.3 Модификаторы

Shape:

- `Cube`: `+100 HP`, `+10 ATK`
- `Sphere`: `+50 HP`, `+20 ATK`

Size:

- `Big`: `+50 HP`
- `Small`: `-50 HP`

Color:

- `Blue`: `-15 ATK`, `+4 ATKSPD`, `+10 SPEED`
- `Green`: `-100 HP`, `+20 ATK`, `-5 SPEED`
- `Red`: `+200 HP`, `+40 ATK`, `-9 SPEED`

## 4.4 Масштабируемость

Чтобы легко добавлять новые цвета/формы:

- Не `switch` по enum в бизнес-логике.
- Использовать таблицы модификаторов (`UnitTraitCatalog`):
  - `Dictionary<UnitShape, StatModifier>`
  - `Dictionary<UnitSize, StatModifier>`
  - `Dictionary<UnitColor, StatModifier>`
- Источник таблиц: `ScriptableObject` конфиг + валидатор на старте.

---

## 5. Сценарий симуляции

Состояния:

1. `Preparation`
2. `Running`
3. `Finished`

Тик боя (`Running`) в фиксированном шаге:

1. Обновить список живых юнитов.
2. Перевыбрать цель для юнитов без валидной цели.
3. Переместить юнитов к цели (`SPEED` units/sec).
4. Если в melee range и cooldown готов — нанести урон `ATK`.
5. Удалить погибших (`HP <= 0`), обновить счетчики.
6. Проверить условие победы.

Правило `ATKSPD` из ТЗ:

- Чем больше `ATKSPD`, тем медленнее атака.
- Формула cooldown: `cooldownSeconds = baseAttackDelay * ATKSPD`.

## 5.1 Навигация и pathfinding

Подход: 2-слойная навигация.

1. Глобальный маршрут (`IPathfinder`):

- Для текущего ТЗ (плоская арена без обязательных препятствий): `DirectPathfinder`.
  - Возвращает путь из 1 waypoint: позиция цели.
- На будущее (без изменения остального кода): `NavMeshPathfinder`.
  - Работает через адаптер `INavMeshService` (`CalculatePath`), а не прямой вызов Unity API из домена.

2. Локальное управление движением (`ISteeringService`):

- Движение к `nextWaypoint`.
- Избегание соседей через spatial hash (uniform grid), чтобы не слипались.
- Вход в melee-range и остановка.
- `ISlotAllocator`: назначение разных слотов вокруг цели (кольцо), чтобы юниты не бежали в одну точку.

Правила репаса:

- Цель умерла -> мгновенный ретаргет + пересчет пути.
- Цель сместилась дальше порога `RepathDistanceThreshold` -> пересчет пути.
- Потеря валидного пути -> fallback на `DirectPathfinder`.

Почему так:

- Для 40 юнитов решение дешевое и стабильное для мобилок.
- Полностью покрывает текущее ТЗ.
- Архитектурно расширяется до препятствий/сложных арен без переписывания боевого цикла.

---

## 6. Дополнительная фича (выбранная)

`God's Wrath`:

- Во время автобоя только у стороны игрока накапливается заряд скилла за убийства вражеских юнитов.
- Когда шкала заполнена, игрок может сделать drag с иконки скилла (UI Toolkit) на поле боя.
- В точке отпускания вызывается `circle AoE`: удар с неба через VFX.
- Урон получают все юниты в радиусе, и свои, и чужие (`friendly fire`).

Минимальные правила v1:

- `WrathCharge += ChargePerKill` за каждый kill стороны-владельца.
- Каст доступен только при `WrathCharge >= MaxCharge`.
- На release drag создается `WrathCastCommand(center, radius, damage)`.
- Урон применяется в момент импакта VFX (`ImpactDelaySeconds`).
- После успешного каста `WrathCharge = 0`.
- Если release вне валидной зоны поля, каст отменяется, заряд сохраняется.
- AI не использует `God's Wrath` в v1.

Стартовые значения для прототипа (тюнятся в конфиге):

- `ChargePerKill = 20`
- `MaxCharge = 100`
- `Radius = 4.0`
- `Damage = 80` (`X` из описания фичи)
- `ImpactDelaySeconds = 0.35`

Архитектурно:

- `Domain`:
  - `WrathMeter` (текущее/максимальное значение).
  - `WrathService` (accumulate, can-cast, consume, apply AoE).
  - `IWrathTargetValidator` (валидация точки на поле).
- `Application`:
  - `OnUnitKilledUseCase` обновляет заряд.
  - `TryCastWrathUseCase` создает команду каста.
- `Presentation`:
  - `WrathViewModel` (`ChargeNormalized`, `CanCast`, `IsDragging`, `PreviewCenter`).
  - Drag pipeline `InputAction -> R3 stream -> VM -> UseCase`.

Почему эта фича:

- Сильный game feel и понятная интеракция для тестового.
- Хорошо тестируется как pure-логика (заряд/урон) и как интеграция (drag/cast/VFX).

---

## 7. TDD стратегия

## 7.1 Test pyramid

- Много `EditMode` unit tests (Domain/Application).
- Небольшой слой `PlayMode` интеграционных тестов.
- Минимум ручного smoke перед демо.

## 7.2 Правило цикла

На каждую задачу:

1. `RED`: пишем тест, который падает.
2. `GREEN`: минимальный код, чтобы тест прошел.
3. `REFACTOR`: чистим API/дубли, не ломая тесты.

## 7.3 Критерии качества тестов

- Детеминированность (фиксируем seed/random provider).
- Никакой зависимости от FPS.
- Не использовать реальные таймеры/WaitForSeconds в unit tests.

---

## 8. Backlog TDD (итерации)

## Iteration 0 — Test Harness и инфраструктура

RED:

- Тесты инициализации DI контейнера и базовых сервисов.
- Тесты сборки конфигов trait-каталога.

GREEN:

- Создать asmdef:
  - `Game.Domain`
  - `Game.Application`
  - `Game.Infrastructure`
  - `Game.Presentation`
  - `Game.Tests.EditMode`
  - `Game.Tests.PlayMode`
- Подключить Reflex/R3 references.

REFACTOR:

- Убрать циклические ссылки asmdef.

## Iteration 1 — Расчет характеристик юнита

RED:

- `StatsCalculator_WhenCubeBigRed_ReturnsExpectedStats`
- `StatsCalculator_WhenSphereSmallBlue_ReturnsExpectedStats`
- `StatsCalculator_WhenNegativeResult_ClampsToMinAllowed`

GREEN:

- `StatBlock`, `StatModifier`, `StatsCalculator`.

REFACTOR:

- Унифицировать сложение модификаторов через aggregator.

## Iteration 2 — Генерация армий и randomization

RED:

- `ArmyFactory_Generates20Units`
- `ArmyRandomizer_WithSameSeed_GeneratesSameComposition`
- `ArmyRandomizer_ChangesComposition_AfterNewSeed`

GREEN:

- `ArmyFactory`, `IRandomProvider`, `ArmyRandomizationUseCase`.

REFACTOR:

- Исключить прямой вызов `UnityEngine.Random` из domain/application.

## Iteration 3 — Таргетинг

RED:

- `TargetSelector_ChoosesNearestAliveEnemy`
- `TargetSelector_ReacquiresWhenTargetDies`
- `TargetSelector_ReturnsNoneWhenNoEnemies`

GREEN:

- `ITargetSelector`, `NearestTargetSelector`.

REFACTOR:

- Выделить метрики дистанции в отдельный helper.

## Iteration 4 — Навигация: path + steering + melee entry

RED:

- `DirectPathfinder_ReturnsSingleWaypoint_TargetPosition`
- `MovementService_FollowsWaypoint_WithSpeedPerSecond`
- `MovementService_StopsAtMeleeRange`
- `SteeringService_AvoidsNeighbors_WithSpatialHash`
- `SlotAllocator_AssignsUniqueSlots_AroundTarget`
- `MovementService_Repaths_WhenTargetMovedBeyondThreshold`
- `MovementService_DoesNothing_WhenNoTarget`

GREEN:

- `IPathfinder`, `DirectPathfinder`.
- `ISteeringService`, `SpatialHashSteeringService`.
- `ISlotAllocator`, `RingSlotAllocator`.
- `MovementService` и конфиги `MeleeRange`, `RepathDistanceThreshold`, `SteeringRadius`.

REFACTOR:

- Убрать дубли вычисления направления/дистанции.
- Изолировать NavMesh-часть в `INavMeshService` для будущего `NavMeshPathfinder` и тестирования через mock.

## Iteration 5 — Атака, кулдауны, смерть

RED:

- `AttackService_AppliesAtkDamage`
- `AttackService_RespectsAtkSpdCooldown`
- `AttackService_DoesNotAttackWhenOutOfRange`
- `HealthService_UnitDiesAtZeroHp`

GREEN:

- `AttackService`, `CooldownTracker`, `HealthService`.

REFACTOR:

- Разделить readonly состояние и мутирующие команды.

## Iteration 6 — Battle loop и победа

RED:

- `BattleLoop_FinishesWhenArmyDestroyed`
- `BattleLoop_ProducesWinnerSide`
- `BattleLoop_DoesNotAdvanceWhenStateNotRunning`

GREEN:

- `BattleLoopService`, `BattleStateMachine`.

REFACTOR:

- Изолировать шаг симуляции как pure method.

## Iteration 7 — Доп. фича God's Wrath

RED:

- `WrathCharge_Increases_OnEnemyKill`
- `WrathCast_Available_OnlyWhenMeterFull`
- `WrathCast_DealsDamage_ToBothSides_InRadius`
- `WrathCast_ConsumesFullMeter_OnSuccess`
- `WrathCast_DoesNotConsumeMeter_OnInvalidTarget`
- `WrathCast_AppliesDamage_OnImpactDelay_NotOnDragRelease`
- `WrathCast_NotAvailable_ForAIController`

GREEN:

- `WrathMeter`, `WrathService`, `TryCastWrathUseCase`.
- `WrathConfig` (`ChargePerKill`, `MaxCharge`, `Radius`, `Damage`, `ImpactDelaySeconds`).
- `WrathTargetValidator` и command model `WrathCastCommand`.

REFACTOR:

- Изолировать AoE overlap query в отдельный сервис (`IUnitQueryInRadius`) для тестируемости.
- Нормализовать пайплайн combat events (`UnitKilled`, `WrathCastStarted`, `WrathImpactApplied`).

## Iteration 8 — Presentation (R3 + UI Toolkit + Input)

RED:

- `PreparationViewModel_RandomizeLeft_UpdatesArmyPreview`
- `PreparationViewModel_StartCommand_ChangesStateToRunning`
- `BattleHudViewModel_UpdatesAliveCounters`
- `WrathViewModel_ShowsChargeProgress_AndCanCastState`
- `WrathDragInput_OnRelease_InsideArena_SendsCastCommand`
- `WrathDragInput_OnRelease_OutsideArena_CancelsCast`
- `ResultViewModel_ShowsWinnerAndReturnToMenu`

GREEN:

- ViewModels на `R3`.
- Binding adapters для UI Toolkit.
- Input adapter (`InputAction` -> `Subject<Unit>`).

REFACTOR:

- Единый binder utility для UI Toolkit элементов.

## Iteration 9 — Интеграционные PlayMode тесты

RED:

- `PlayMode_StartBattle_RunsToCompletion`
- `PlayMode_RandomizeBeforeStart_ChangesBothArmies`
- `PlayMode_WrathDragAndCast_DealsFriendlyFireInRadius`
- `PlayMode_DeathRagdoll_Fades_AndDespawns`
- `PlayMode_AfterBattle_ReturnsToMenu`

GREEN:

- Scene bootstrap + Reflex scopes + ui navigation.

REFACTOR:

- Упростить создание тестовой сцены/фикстур.

## Iteration 10 — Game feel polish (после функционала)

RED (smoke критерии):

- hit feedback появляется при уроне.
- death ragdoll срабатывает при смерти (`Rigidbody.isKinematic = false`).
- death impulse отталкивает юнита при смерти.
- fade VFX запускается после смерти и завершает despawn.
- `God's Wrath` имеет telegraph circle preview и читаемый impact VFX.
- UI обновляется без лагов/рывков.

GREEN:

- Death pipeline:
  - логическая смерть в домене;
  - в presentation/adapters юнит переводится в ragdoll;
  - запускается fade VFX;
  - object despawn в пул после `DeathDespawnDelay`.
- VFX/анимации через pooling.
- Тайминги эффектов от боевого state stream (`R3`).
- DOTween используется только в presentation-слое (telegraph, hit/fade tweens, UI juiciness), без зависимости domain/application от DOTween API.

REFACTOR:

- Ограничить аллокации в рантайме (проверка profiler).
- Разделить `DeathPresentationService` и `DeathCleanupService`, чтобы визуал не влиял на боевую логику.

---

## 9. Структура папок

```text
Assets/
  Game/
    Domain/
      Combat/
      Army/
      Traits/
      Skills/
    Application/
      UseCases/
      Services/
      StateMachine/
    Infrastructure/
      DI/
      Random/
      Time/
      Scene/
    Presentation/
      UI/
        UXML/
        USS/
      ViewModels/
      Input/
      Binding/
      Death/
    Config/
      Traits/
      Combat/
      Skills/
    Tests/
      EditMode/
      PlayMode/
```

---

## 10. Definition of Done

Фича считается завершенной, если:

- Все тесты `EditMode`/`PlayMode` зелёные.
- Нет compile errors при чистом открытии проекта.
- Можно:
  - зайти в preparation,
  - рандомизировать армии,
  - стартовать бой,
  - дождаться победителя,
  - вернуться в меню.
- Доп. фича `God's Wrath` работает и покрыта тестами:
  - заряд от убийств;
  - drag-target по полю;
  - AoE урон по своим и чужим;
  - корректный сброс заряда.
  - AI не кастует `God's Wrath`.
- Death game feel готов:
  - ragdoll (`non-kinematic rigidbody`) при смерти;
  - отлет;
  - fade VFX;
  - корректный despawn.
- Есть `README.md` с:
  - описанием архитектуры,
  - объяснением выбора Reflex/UI Toolkit/R3,
  - ограничениями и trade-offs,
  - временем, затраченным на ТЗ.

---

## 11. Риски и ранние решения

Риски:

- Неопределенность формулы `ATKSPD` (принято: больше = дольше cooldown).
- Перегрузка UI реактивными подписками.
- Тесты, завязанные на реальные Unity timing APIs.
- Конфликт между логическим удалением юнита и визуальным ragdoll lifecycle.
- Нагрузка physics/VFX при массовой смерти и AoE-кастах.
- Несинхронность DOTween-анимаций и боевого state stream.

Контрмеры:

- Явно зафиксировать формулу в README и тестах.
- Централизованный disposable lifecycle.
- Абстракция `ITimeProvider` + deterministic tick в unit tests.
- Развести `IsAlive` (domain) и `IsPresented` (view lifecycle) как разные состояния.
- Pooling тел/эффектов и лимиты одновременных VFX.
- Все DOTween tween'ы привязывать к lifecycle owner и убивать на despawn/screen dispose.

---

## 12. Порядок запуска разработки

1. Подключить Reflex + R3.
2. Создать asmdef и тестовые сборки.
3. Пройти Iteration 1-6 (ядро боя).
4. Пройти Iteration 7 (доп. фича).
5. Пройти Iteration 8-9 (UI, интеграция).
6. Iteration 10 (game feel).
7. Финализировать README и оценку времени.
