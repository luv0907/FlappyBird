#!/usr/bin/env bash
set -euo pipefail

# Usage: ./scripts/build_android.sh /path/to/Unity [BuildMethod]
# Example: ./scripts/build_android.sh "/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity" BuildMobile.BuildAndroidAPK

UNITY_BIN="${1:-}" 
BUILD_METHOD="${2:-BuildMobile.BuildAndroidAPK}"

if [ -z "$UNITY_BIN" ]; then
  echo "Usage: $0 /path/to/Unity [BuildMethod]"
  exit 1
fi

PROJECT_PATH="$(pwd)"
BUILD_DIR="$PROJECT_PATH/Builds/Android"
mkdir -p "$BUILD_DIR"

echo "Running Unity: $UNITY_BIN"
"$UNITY_BIN" -quit -batchmode -projectPath "$PROJECT_PATH" -executeMethod "$BUILD_METHOD" -logFile "$BUILD_DIR/unity_build.log"

echo "Build finished. Check $BUILD_DIR for outputs and unity_build.log for logs."
