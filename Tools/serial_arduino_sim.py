#!/usr/bin/env python3
import os
import sys
import time
import platform

IS_WINDOWS = platform.system() == "Windows"
DEFAULT_PORT = 'COM11' if IS_WINDOWS else '/dev/pts/4'
DEFAULT_INVENTORY = 'INVENTORY:SENSOR:PIR_HOME:2,SENSOR:PIR_AWAY:3,LED:5,LED:6,LED:7,LED:8'
try:
    import serial  # pyright: ignore[reportMissingImports]
except Exception:
    print('pyserial no estÃ¡ instalado. InstÃ¡lalo con: pip install pyserial')
    sys.exit(1)

PORT = os.environ.get('SERIAL_PORT', DEFAULT_PORT)
BAUD = int(os.environ.get('SERIAL_BAUD', '9600'))

ser = None


def open_port():
    global ser
    if ser and ser.is_open:
        print('Puerto ya abierto')
        return
    try:
        port = PORT
        if IS_WINDOWS and not port.startswith('\\\\.\\') and port.upper().startswith('COM') and len(port) > 4:
            port = '\\\\.\\' + port
        ser = serial.Serial(port, BAUD, timeout=1)
        time.sleep(0.1)
        print(f'Abierto {PORT} @ {BAUD}')
    except Exception as e:
        print('Error al abrir puerto:', e)
        ser = None


def close_port():
    global ser
    if ser:
        try:
            ser.close()
        except:
            pass
        ser = None
        print('Puerto cerrado')


def send_line(line):
    if not ser or not ser.is_open:
        print('Puerto no abierto. Usa la opciÃ³n para abrirlo.')
        return
    try:
        if not line.endswith('\n'):
            line = line + '\n'
        ser.write(line.encode('utf-8'))
        ser.flush()
        print('->', line.strip())
    except Exception as e:
        print('Error escribiendo:', e)


def handle_incoming(line):
    """Auto-responde a comandos del C#"""
    print('<<', line)

    if line == 'REQUEST_INVENTORY':
        send_line(DEFAULT_INVENTORY)

    elif line.startswith('LED_ON:') or line.startswith('LED_OFF:'):
        print('  [LED]', line.split(':')[0], 'pin', line.split(':')[1])

    elif line.startswith('LCD_SCORES:'):
        parts = line.split(':')
        print(f'  [LCD] Scores: LOCAL={parts[1]} - AWAY={parts[2]}')

    elif line.startswith('SENSOR_UPDATE:') or line.startswith('SENSOR_TRIGGER:'):
        pass


def prompt(text):
    try:
        return input(text).strip()
    except EOFError:
        print('\nEntrada cerrada. Saliendo.')
        raise SystemExit(0)


def main_loop():
    open_port()
    print('Esperando comandos por el puerto serial')
    try:
        while True:
            if ser and ser.is_open:
                data = ser.readline()
                if not data:
                    continue
                try:
                    line = data.decode('utf-8', errors='ignore').strip()
                except Exception:
                    continue
                if not line:
                    continue
                handle_incoming(line)
            else:
                time.sleep(0.2)
    except KeyboardInterrupt:
        close_port()


def menu():
    global PORT, BAUD
    while True:
        print('\nSERIAL ARDUINO SIM')
        print('Port:', PORT, 'Baud:', BAUD)
        print('1) Open port')
        print('2) Close port')
        print('3) Send inventory')
        print('4) Trigger PIR (SENSOR_TRIGGER)')
        print('5) Send sensor update (SENSOR_UPDATE)')
        print('6) Change port/baud')
        print('7) Exit')
        choice = prompt('> ')
        if choice == '1':
            open_port()
        elif choice == '2':
            close_port()
        elif choice == '3':
            inv = prompt('Inventory string (or leave empty for default)\n> ')
            if not inv:
                inv = DEFAULT_INVENTORY
            send_line(inv)
        elif choice == '4':
            which = prompt('Which PIR (PIR_HOME or PIR_AWAY or PIR)\n> ') or 'PIR'
            send_line(f'SENSOR_TRIGGER:{which}')
        elif choice == '5':
            parts = prompt('Format: SENSOR_UPDATE:TYPE:VALUE (e.g. SENSOR_UPDATE:PIR_HOME:1.0)\n> ')
            if parts:
                send_line(parts)
        elif choice == '6':
            p = prompt('Port (current ' + PORT + ') > ')
            b = prompt('Baud (current ' + str(BAUD) + ') > ')
            if p:
                PORT = p
            if b:
                try:
                    BAUD = int(b)
                except:
                    pass
            print('Settings updated')
        elif choice == '7':
            close_port()
            sys.exit(0)
        else:
            print('Invalid')


if __name__ == '__main__':
    print(f'Plataforma: {platform.system()} | Usa SERIAL_PORT y SERIAL_BAUD env vars')
    print(f'Puerto por defecto: {DEFAULT_PORT}')
    if len(sys.argv) > 1 and sys.argv[1] == '--menu':
        menu()
    elif len(sys.argv) > 1 and sys.argv[1] == '--auto':
        main_loop()
    elif sys.stdin.isatty():
        menu()
    else:
        main_loop()
