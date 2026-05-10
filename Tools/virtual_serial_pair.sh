#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PIPES_DIR="$ROOT_DIR/.virtual-serial"

echo "Iniciando puertos virtuales con socat..."
echo ""

# Crear directorio si no existe
mkdir -p "$PIPES_DIR"

# Limpiar symlinks antiguos
rm -f "$PIPES_DIR/csharp" "$PIPES_DIR/python"

# Crear FIFOs temporales para capturar nombres de puertos
TEMP_LOG=$(mktemp)
trap "rm -f $TEMP_LOG" EXIT

# Lanzar socat y capturar output
socat -d -d pty,raw,echo=0 pty,raw,echo=0 2>&1 | tee "$TEMP_LOG" &
SOCAT_PID=$!

# Esperar a que socat cree los puertos
sleep 1

# Extraer los puertos de la salida de socat
CSHARP_PORT=$(grep -oP 'PTY is \K/dev/pts/\d+' "$TEMP_LOG" 2>/dev/null | head -1 || echo "")
PYTHON_PORT=$(grep -oP 'PTY is \K/dev/pts/\d+' "$TEMP_LOG" 2>/dev/null | tail -1 || echo "")

# Fallback
[ -z "$CSHARP_PORT" ] && CSHARP_PORT="/dev/pts/3"
[ -z "$PYTHON_PORT" ] && PYTHON_PORT="/dev/pts/4"

echo "✓ Puertos virtuales creados:"
echo "  PTY 1: $CSHARP_PORT"
echo "  PTY 2: $PYTHON_PORT"
echo ""

# Crear symlinks en .virtual-serial/ para facilitar el acceso
ln -sf "$CSHARP_PORT" "$PIPES_DIR/csharp"
ln -sf "$PYTHON_PORT" "$PIPES_DIR/python"

echo "✓ Symlinks creados en $PIPES_DIR/"
echo "  .virtual-serial/csharp -> $CSHARP_PORT"
echo "  .virtual-serial/python -> $PYTHON_PORT"
echo ""

# Actualizar .env con ambas variantes (directo y symlink)
if [ -f "$ROOT_DIR/.env" ]; then
    sed -i "s|^COM_PORT=.*|COM_PORT=$CSHARP_PORT|" "$ROOT_DIR/.env"
    sed -i "s|^SERIAL_PORT=.*|SERIAL_PORT=$PYTHON_PORT|" "$ROOT_DIR/.env"
fi

echo "✓ Archivo .env actualizado"
echo ""
echo "Esperando 2 segundos más para que los puertos estén completamente listos..."
sleep 2
echo ""
echo "✅ Los puertos están listos. Puedes iniciar C# y Python."
echo "Presiona Ctrl+C para detener socat."
echo ""

# Mantener socat ejecutándose
wait $SOCAT_PID


