# Documentación del Diseño UML — Estadio El Campín
## Sistema de Gestión Inteligente de Público y Modos de Evento

---

## 1. Descripción General del Sistema

El sistema gestiona el comportamiento del Estadio El Campín mediante tres modos de operación: **PARTIDO**, **MANTENIMIENTO** y **EMERGENCIA**. Controla sensores de presencia y luz, zonas de iluminación, un marcador LED y un sistema de sonido (buzzer), todo orquestado a través de una arquitectura orientada a eventos con principios GRASP aplicados.

---

## 2. Arquitectura General

El diagrama se organiza en cuatro capas lógicas:

**Capa de control** — `StadiumController`, `SoundController`, `ScoreController`, `LightController`  
**Capa de dominio** — `Stadium`, `Scoreboard`, `Light`  
**Capa de dispositivos** — `LED`, `Buzzer`, `ScreenLCD`, `SensorPIR`, `SensorLDR`  
**Capa de infraestructura** — `EventBus`, `ArduinoConnection`, `DeviceFactory`, `AdminSession`

---

## 3. Clases del Sistema

### 3.1 Stadium
Representa el estadio como entidad central del dominio. Mantiene el estado actual del sistema y posee las referencias a los dispositivos de alto nivel.

**Atributos:**
- `- actualState: StadiumStates` — estado actual (MATCH, MAINTENANCE, EMERGENCY)
- `- lights: Light[]` — arreglo de zonas de iluminación
- `- scoreboard: Scoreboard` — marcador del estadio

**Métodos:**
- `+ changeState(StadiumStates): void` — cambia el modo del estadio
- `+ getLights(): Light[]` — retorna las luces
- `+ getScoreboard(): Scoreboard` — retorna el marcador
- `+ getState(): StadiumStates` — retorna el estado actual

---

### 3.2 StadiumController
Es el **controlador GRASP** del sistema. Orquesta los tres modos de operación delegando a los sub-controladores especializados. Recibe eventos del `EventBus` y decide qué acción tomar.

**Atributos:**
- `- stadium: Stadium`
- `- soundController: SoundController`
- `- scoreController: ScoreController`
- `- lightController: LightController`

**Métodos:**
- `+ setMode(state: StadiumStates): void` — punto de entrada para cambio de modo
- `+ activateMatchMode(): void` — enciende luces y marcador, reproduce sonido de partido
- `+ activateMaintenanceMode(): void` — luces tenues, marcador apagado, sin sonido
- `+ activateEmergencyMode(): void` — luces intermitentes, alarma de emergencia
- `+ handle(event: IEvent): void` — reacciona a eventos del bus

---

### 3.3 SoundController
Responsable de todo el comportamiento de audio del estadio. Implementa `IEventHandler` para reaccionar a eventos relevantes.

**Atributos:**
- `+ currentTrack: SoundTracks` — pista de audio actual
- `- audioDevice: IAudioDevice` — dispositivo de audio (abstracción)

**Métodos:**
- `+ setGlobalVolume(double): void`
- `+ play(SoundTracks): void`
- `+ stop(SoundTracks): void`
- `+ handle(event: IEvent): void`

---

### 3.4 ScoreController
Gestiona el marcador del partido. Tiene acceso directo al `Scoreboard` para actualizarlo.

**Atributos:**
- `- scoreboard: Scoreboard`

**Métodos:**
- `+ resetScores(): void`
- `+ setScoreboard(Scoreboard): void`
- `+ incrementLocalScore(): void`
- `+ incrementAwayScore(): void`
- `+ handle(event: IEvent): void`

---

### 3.5 LightController
Controla todas las zonas de iluminación del estadio. Implementa `IEventHandler`.

**Atributos:**
- `- lights: Light[]`

**Métodos:**
- `+ turnLightsOn(): void`
- `+ turnLightsOff(): void`
- `+ blinkLights(): void` — para modo emergencia
- `+ emergencyLights(): void`
- `+ handle(event: IEvent): void`

---

### 3.6 Scoreboard
Representa el marcador del estadio. Gestiona los puntajes y se comunica con la pantalla a través de la interfaz `IScoreDisplay`, sin acoplarse a una implementación concreta.

**Atributos:**
- `- scoreLocal: int`
- `- scoreAway: int`
- `- display: IScoreDisplay` — abstracción de la pantalla

**Métodos:**
- `+ setScore(team: String, newScore: int): void`
- `+ getScores(): {int}`
- `+ setDisplay(IScoreDisplay): void`

---

### 3.7 Light
Representa una zona de iluminación. Encapsula un `LED` como dispositivo físico.

**Atributos:**
- `- led: LED`

**Métodos:**
- `+ on(): void`
- `+ off(): void`
- `+ isOn(): boolean`
- `+ setLed(LED): void`

---

### 3.8 EventBus
Implementa el patrón Publicador/Suscriptor. Es un Singleton que desacopla completamente a quienes publican eventos de quienes los consumen.

**Atributos:**
- `- subscribers` — mapa de suscriptores por tipo de evento
- `- instance` — instancia única (Singleton)

**Métodos:**
- `- EventBus()` — constructor privado
- `+ getInstance(): EventBus`
- `+ publish(event: IEvent): void`
- `+ subscribe(eventType, handler: Handler): void`
- `+ unsubscribe(eventType, handler: Handler): void`

---

### 3.9 ArduinoConnection
Gestiona la comunicación serial con el hardware Arduino. Es un Singleton que escucha mensajes entrantes y los convierte en eventos del sistema publicados en el `EventBus`.

**Atributos:**
- `- port: SerialPort`
- `- eventBus: EventBus`
- `- instance: ArduinoConnection`

**Métodos:**
- `+ getInstance(): ArduinoConnection`
- `+ sendCommand(command: ICommand): void`
- `+ startListening(): void`
- `+ processIncomingMessage(String): void`
- `+ processInventory(String): void`

---

### 3.10 DeviceFactory
Fábrica de dispositivos. Centraliza la creación de sensores y actuadores, retornando siempre interfaces, no implementaciones concretas.

**Métodos:**
- `+ createSensor(type: String, pin: String): ISensor`
- `+ createActuator(type: String, pin: String): IActuator`

---

### 3.11 AdminSession
Gestiona la autenticación del administrador del sistema. Solo un usuario autenticado puede cambiar el modo del estadio.

**Atributos:**
- `- username: String`
- `- isAuthenticated: boolean`

**Métodos:**
- `+ login(user: String, pass: String): boolean`
- `+ logout(): void`
- `+ isAuthenticated(): boolean`

---

### 3.12 Dispositivos físicos

| Clase | Tipo | Implementa | Descripción |
|---|---|---|---|
| `SensorPIR` | Sensor | `ISensor` | Detecta presencia (- detected: boolean) |
| `SensorLDR` | Sensor | `ISensor` | Mide nivel de luz (- lightLevel: double) |
| `LED` | Actuador | `IActuator` | Control individual de LED |
| `Buzzer` | Actuador | `IActuator`, `IAudioDevice` | Genera sonidos y alarmas |
| `ScreenLCD` | Actuador | `IActuator`, `IScoreDisplay` | Pantalla del marcador |

---

## 4. Interfaces del Sistema

### ISensor
Define el contrato para todos los sensores.
```
+ read(): double
+ updateValue(double): void
```

### IActuator
Define el contrato para todos los actuadores.
```
+ isOn: boolean
+ on(): void
+ off(): void
```

### IEventHandler
Define el contrato para cualquier clase que reaccione a eventos.
```
+ handle(event: IEvent): void
```

### IEvent
Define el contrato base para todos los eventos del sistema.
```
+ getType(): String
```

### ICommand
Define el contrato para comandos enviados al Arduino.
```
+ name: String
+ data
```

### IAudioDevice
Abstrae el dispositivo de audio, permitiendo cambiar el `Buzzer` por otro dispositivo sin modificar `SoundController`.
```
+ play(SoundTracks): void
+ stop(SoundTracks): void
+ setVolume(double): void
```

### IScoreDisplay
Abstrae la pantalla del marcador, permitiendo cambiar `ScreenLCD` por otra tecnología sin modificar `Scoreboard`.
```
+ showScores(scoreLocal: int, scoreAway: int): void
+ showTemporaryMessage(message: String, durationMs: int): void
+ clearDisplay(): void
```

---

## 5. Enumeraciones

### StadiumStates
Define los tres modos de operación del estadio:
- `MATCH` — Modo partido activo
- `MAINTENANCE` — Modo mantenimiento
- `EMERGENCY` — Modo emergencia

### SoundTracks
Define las pistas de audio disponibles:
- `GOAL` — Sonido de gol
- `EMERGENCY` — Alarma de emergencia
- `MATCH` — Música de partido
- `MAINTENANCE` — Señal de mantenimiento

---

## 6. Flujo de Funcionamiento

### 6.1 Cambio de modo
1. El Arduino detecta un evento (botón físico, sensor) y lo envía por puerto serial
2. `ArduinoConnection` recibe el mensaje en `processIncomingMessage()`
3. `ArduinoConnection` publica un `IEvent` en el `EventBus`
4. `StadiumController` está suscrito al `EventBus` y su `handle()` recibe el evento
5. `StadiumController` llama a `setMode(StadiumStates)` que internamente invoca el método de modo correspondiente
6. Cada método de modo llama a `LightController`, `SoundController` y `ScoreController` para ejecutar los comportamientos requeridos

### 6.2 Modo PARTIDO
- `LightController.turnLightsOn()` — enciende todas las zonas
- `ScoreController.resetScores()` — reinicia el marcador
- `SoundController.play(SoundTracks.MATCH)` — reproduce música
- `Stadium.changeState(MATCH)`

### 6.3 Modo MANTENIMIENTO
- `LightController.turnLightsOff()` — apaga luces no esenciales
- `SoundController.stop(...)` — detiene el audio
- `Stadium.changeState(MAINTENANCE)`

### 6.4 Modo EMERGENCIA
- `LightController.emergencyLights()` — luces intermitentes
- `SoundController.play(SoundTracks.EMERGENCY)` — alarma sonora
- `Stadium.changeState(EMERGENCY)`

---

## 7. Principios GRASP Aplicados

### 7.1 Controlador (Controller)
**Clase:** `StadiumController`

`StadiumController` es el controlador GRASP del sistema. No ejecuta lógica de negocio directamente sino que **delega** a los controladores especializados (`SoundController`, `LightController`, `ScoreController`). Actúa como punto de entrada para los eventos del sistema y coordina los modos de operación.

```
ArduinoConnection → EventBus → StadiumController → [sub-controladores]
```

### 7.2 Experto en Información (Information Expert)
**Clases:** `Stadium`, `Scoreboard`, `Light`

Cada clase gestiona la información que le pertenece naturalmente:
- `Stadium` es el experto en el estado del estadio → él gestiona `actualState`
- `Scoreboard` es el experto en los puntajes → él gestiona `scoreLocal` y `scoreAway`
- `Light` es el experto en su propio estado de iluminación → ella controla su `LED`

### 7.3 Creador (Creator)
**Clase:** `DeviceFactory`

`DeviceFactory` centraliza la creación de todos los dispositivos del sistema. Aplica el patrón Factory Method: quien mejor conoce qué instancia crear (por tipo y pin) es la fábrica. Retorna siempre `ISensor` o `IActuator`, nunca clases concretas.

### 7.4 Bajo Acoplamiento (Low Coupling)
**Mecanismos:**
- `ISensor` e `IActuator` desacoplan los controladores de los dispositivos físicos
- `IAudioDevice` desacopla `SoundController` del `Buzzer` concreto
- `IScoreDisplay` desacopla `Scoreboard` de `ScreenLCD`
- `EventBus` desacopla completamente a `ArduinoConnection` de los controladores
- `IEventHandler` permite agregar nuevos suscriptores sin modificar el bus

### 7.5 Alta Cohesión (High Cohesion)
**Clases:** `SoundController`, `ScoreController`, `LightController`

Cada controlador tiene **una única responsabilidad**:
- `SoundController` → solo gestiona audio
- `ScoreController` → solo gestiona puntajes
- `LightController` → solo gestiona iluminación

`StadiumController` coordina pero no implementa directamente ninguno de estos comportamientos.

### 7.6 Polimorfismo (Polymorphism)
**Interfaces:** `ISensor`, `IActuator`, `IEventHandler`, `IAudioDevice`, `IScoreDisplay`

El sistema puede trabajar con cualquier sensor sin preguntar de qué tipo es. `DeviceFactory` retorna `ISensor` y el sistema llama `read()` sin saber si es un PIR o un LDR. Igualmente, `SoundController` llama `play()` sobre `IAudioDevice` sin saber si es un buzzer o un altavoz bluetooth.

### 7.7 Indirección (Indirection)
**Clase:** `EventBus`

`EventBus` actúa como intermediario entre `ArduinoConnection` y los controladores. Ninguno se conoce directamente — se comunican a través del bus. Esto permite agregar o quitar suscriptores sin modificar quien publica.

### 7.8 Variaciones Protegidas (Protected Variations)
**Puntos de variación protegidos:**

| Variación posible | Protegida por |
|---|---|
| Cambiar tipo de sensor | `ISensor` |
| Cambiar tipo de actuador | `IActuator` |
| Cambiar dispositivo de audio | `IAudioDevice` |
| Cambiar pantalla del marcador | `IScoreDisplay` |
| Cambiar tipo de evento | `IEvent` |
| Agregar nuevo suscriptor | `IEventHandler` |
| Cambiar protocolo de comunicación | `ICommand` |

### 7.9 Singleton (Pure Fabrication)
**Clases:** `EventBus`, `ArduinoConnection`

Ambas clases deben existir como instancia única en el sistema. Implementan el patrón Singleton con constructor privado y `getInstance()` público. Esto garantiza que todos los componentes usen el mismo bus de eventos y la misma conexión serial.

---

## 8. Patrones de Diseño Aplicados

| Patrón | Clase(s) | Propósito |
|---|---|---|
| **Singleton** | `EventBus`, `ArduinoConnection` | Instancia única garantizada |
| **Observer** | `EventBus`, `IEventHandler` | Comunicación desacoplada por eventos |
| **Factory Method** | `DeviceFactory` | Creación centralizada de dispositivos |
| **Strategy** (implícito) | `IAudioDevice`, `IScoreDisplay` | Intercambio de implementaciones en tiempo de ejecución |

---

## 9. Resumen de Cumplimiento de Requerimientos

| Requerimiento | Clase responsable |
|---|---|
| Sensor de presencia/ocupación | `SensorPIR` (detecta presencia) |
| Sensor de luz | `SensorLDR` (mide nivel de luz) |
| Zonas de iluminación | `Light` + `LED` + `LightController` |
| LEDs de marcador | `ScreenLCD` + `Scoreboard` + `IScoreDisplay` |
| Zumbador | `Buzzer` + `IAudioDevice` + `SoundController` |
| Modo PARTIDO | `StadiumController.activateMatchMode()` |
| Modo MANTENIMIENTO | `StadiumController.activateMaintenanceMode()` |
| Modo EMERGENCIA | `StadiumController.activateEmergencyMode()` |
| Luces + marcador encendidos en PARTIDO | `LightController` + `ScoreController` coordinados por `StadiumController` |
| Autenticación del administrador | `AdminSession` verificada por `StadiumController` |
