#!/usr/bin/env bash
set -euo pipefail

# packages the built APK(s) into dist/android and optionally installs to a connected device
# Usage: ./scripts/package_and_install.sh [--install]

PROJECT_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
BUILD_DIR="$PROJECT_ROOT/Builds/Android"
DIST_DIR="$PROJECT_ROOT/dist/android"
ZIP_OUT="$PROJECT_ROOT/first-game-android.zip"

INSTALL=false
if [[ ${1:-} == "--install" ]]; then
  INSTALL=true
fi

mkdir -p "$DIST_DIR"

shopt -s nullglob
APK_FILES=($BUILD_DIR/*.apk)
shopt -u nullglob

if [ ${#APK_FILES[@]} -eq 0 ]; then
  echo "No APK found in $BUILD_DIR. Build the project in Unity first."
  exit 1
fi

# If multiple APKs, pick the first one (you can customize this)
APK_SRC="${APK_FILES[0]}"
APK_NAME="game.apk"

echo "Found APK: $APK_SRC"
cp "$APK_SRC" "$DIST_DIR/$APK_NAME"

echo "Writing README into $DIST_DIR"
cat > "$DIST_DIR/README.txt" <<EOF
Install instructions:
- Enable 'Install unknown apps' for your browser/file manager on Android.
- Copy or download 'game.apk' to the Android device and open it to install.
- Or use adb: 'adb install -r game.apk'
EOF

echo "Creating zip archive: $ZIP_OUT"
cd "$PROJECT_ROOT"
zip -r -q "$ZIP_OUT" dist/android
echo "Packaged: $ZIP_OUT"

if [ "$INSTALL" = true ]; then
  if command -v adb >/dev/null 2>&1; then
    echo "Checking connected devices..."
    adb kill-server || true
    adb start-server
    DEVICES=$(adb devices | awk 'NR>1 && $2=="device" {print $1}')
    if [ -z "$DEVICES" ]; then
      echo "No authorized Android devices found. Connect a device and enable USB debugging."
      exit 1
    fi

    echo "Installing on device(s):"
    for d in $DEVICES; do
      echo " - $d"
      adb -s "$d" install -r "$DIST_DIR/$APK_NAME"
    done
    echo "Install complete."
  else
    echo "adb not found in PATH. Install Android Platform Tools and try again."
    exit 1
  fi
fi

echo "Done. dist/android contains the APK and README."
