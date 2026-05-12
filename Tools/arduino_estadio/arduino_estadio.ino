#define PIN_PIR_HOME 2
#define PIN_PIR_AWAY 3

const uint8_t LED_PINS[] = {5, 6, 7, 8};
const uint8_t LED_COUNT = sizeof(LED_PINS) / sizeof(LED_PINS[0]);

uint8_t pirHomeState = LOW;
uint8_t pirAwayState = LOW;
uint8_t lastPirHomeState = LOW;
uint8_t lastPirAwayState = LOW;

unsigned long lastHomeTriggerMs = 0;
unsigned long lastAwayTriggerMs = 0;
const unsigned long COOLDOWN_MS = 4000;

void setup() {
    Serial.begin(9600);
    while (!Serial) { }

    pinMode(PIN_PIR_HOME, INPUT);
    pinMode(PIN_PIR_AWAY, INPUT);

    for (uint8_t i = 0; i < LED_COUNT; i++) {
        pinMode(LED_PINS[i], OUTPUT);
        digitalWrite(LED_PINS[i], LOW);
    }
}

void loop() {
    unsigned long now = millis();
    processSerial();
    readPirSensor(PIN_PIR_HOME, "PIR_HOME", pirHomeState, lastPirHomeState, lastHomeTriggerMs, now);
    readPirSensor(PIN_PIR_AWAY, "PIR_AWAY", pirAwayState, lastPirAwayState, lastAwayTriggerMs, now);
    delay(50);
}

void processSerial() {
    if (Serial.available() == 0) return;
    String line = Serial.readStringUntil('\n');
    line.trim();
    if (line.length() == 0) return;

    if (line.startsWith("REQUEST_INVENTORY")) {
        sendInventory();
    }
    else if (line.startsWith("LED_ON:")) {
        int pin = line.substring(7).toInt();
        setLed(pin, HIGH);
    }
    else if (line.startsWith("LED_OFF:")) {
        int pin = line.substring(8).toInt();
        setLed(pin, LOW);
    }
}

void sendInventory() {
    Serial.print("INVENTORY:");
    Serial.print("SENSOR:PIR_HOME:");  Serial.print(PIN_PIR_HOME); Serial.print(",");
    Serial.print("SENSOR:PIR_AWAY:");  Serial.print(PIN_PIR_AWAY); Serial.print(",");
    for (uint8_t i = 0; i < LED_COUNT; i++) {
        Serial.print("LED:"); Serial.print(LED_PINS[i]);
        if (i < LED_COUNT - 1) Serial.print(",");
    }
    Serial.println();
}

void setLed(int pin, uint8_t state) {
    for (uint8_t i = 0; i < LED_COUNT; i++) {
        if (LED_PINS[i] == pin) {
            digitalWrite(pin, state);
            return;
        }
    }
}

void readPirSensor(uint8_t pin, const char* name,
                   uint8_t& currentState, uint8_t& lastState,
                   unsigned long& lastTriggerMs, unsigned long now) {
    currentState = digitalRead(pin);

    if (currentState != lastState) {
        delay(50);
        currentState = digitalRead(pin);
    }

    if (currentState == HIGH && lastState == LOW) {
        if (now - lastTriggerMs >= COOLDOWN_MS) {
            Serial.print("SENSOR_TRIGGER:");
            Serial.println(name);
            lastTriggerMs = now;
        }
    }

    lastState = currentState;
}