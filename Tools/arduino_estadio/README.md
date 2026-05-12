# Arduino Estadio - Guia de Instalacion

## Hardware necesario

- Arduino UNO (o compatible)
- 2 sensores PIR (HC-SR501 o similar)
- 4 LEDs (cualquier color)
- 4 resistencias de 220Ω
- Cables dupont
- Protoboard (opcional)

## Conexiones

```
Arduino UNO
├── Pin 2  ─── SIGNAL PIR_HOME  (sensor izquierda / local)
├── Pin 3  ─── SIGNAL PIR_AWAY  (sensor derecha / visitante)
├── Pin 5  ─── LED 1 (resistencia 220Ω a GND)
├── Pin 6  ─── LED 2 (resistencia 220Ω a GND)
├── Pin 7  ─── LED 3 (resistencia 220Ω a GND)
├── Pin 8  ─── LED 4 (resistencia 220Ω a GND)
└── GND   ─── GND de sensores PIR y LEDs
```

**PIR**: VCC → 5V, GND → GND, SIGNAL → pin correspondiente

**LED**: Resistencia 220Ω en serie con el ánodo, cátodo a GND

## Instalacion del software

### 1. Instalar Arduino IDE

Descarga desde: https://www.arduino.cc/en/software

### 2. Cargar el programa

1. Abrir `arduino_estadio.ino` en Arduino IDE
2. Seleccionar placa: Herramientas → Placa → Arduino UNO
3. Seleccionar puerto: Herramientas → Puerto → COMX
4. Clic en el boton "Subir" (flecha →)

### 3. Verificar funcionamiento

Abrir el "Monitor Serial" (Herramientas → Monitor Serial) a 9600 baudios.
Enviar `REQUEST_INVENTORY` y presionar Enter.

Deberia aparecer:
```
INVENTORY:SENSOR:PIR_HOME:2,SENSOR:PIR_AWAY:3,LED:5,LED:6,LED:7,LED:8
```

## Configuracion de pines

Editar las primeras lineas del archivo:

```cpp
#define PIN_PIR_HOME 2
#define PIN_PIR_AWAY 3

const uint8_t LED_PINS[] = {5, 6, 7, 8};
```

## Comandos seriales

Desde C# hacia Arduino:

| Comando | Descripcion |
|---------|-------------|
| `REQUEST_INVENTORY` | Devuelve inventario de hardware |
| `LED_ON:<pin>` | Enciende LED en ese pin |
| `LED_OFF:<pin>` | Apaga LED en ese pin |

Desde Arduino hacia C#:

| Evento | Cuando ocurre |
|---------|---------------|
| `SENSOR_TRIGGER:PIR_HOME` | Movimiento en pin 2 |
| `SENSOR_TRIGGER:PIR_AWAY` | Movimiento en pin 3 |

## Comunicacion con la PC

### Windows (USB directo)

1. Conectar Arduino por USB
2. Anotar el COM asignado (ej: COM3)
3. Poner ese valor en `.env` como `COM_PORT=COM3`
4. Ejecutar `dotnet run`

### Windows (Puerto virtual con com0com)

1. Instalar com0com: https://sourceforge.net/projects/com0com/
2. Crear par virtual (ej: COM3 ↔ COM4)
3. Configurar `.env` con los puertos correspondientes

### Linux (PTY con socat)

```bash
./Tools/virtual_serial_pair.sh
```

## Solucion de problemas

**El Arduino no responde:**
- Verificar que el Monitor Serial esté a 9600 baudios
- Verificar que el cable USB esté bien
- Revisar que el codigo se haya subido correctamente

**Los LEDs no parpadean:**
- Revisar las conexiones fisicas
- Verificar las resistencias de 220Ω
- Probar cada LED individualmente con el ejemplo "Blink"

**Los sensores PIR no detectan:**
- Ajustar los potenciometros de sensibilidad y tiempo en el HC-SR501
- Esperar 30-60 segundos para que el sensor se estabilice al energizar
- Moverte frente al sensor y ver si aparece `SENSOR_TRIGGER:...` en el Monitor Serial