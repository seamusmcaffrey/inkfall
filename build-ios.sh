#!/bin/bash
set -euo pipefail

# --- Configuration ---
UNITY_BIN="/Applications/Unity/Hub/Editor/6000.3.10f1/Unity.app/Contents/MacOS/Unity"
PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
BUILD_DIR="${PROJECT_DIR}/Builds/iOS"
LOG_DIR="${PROJECT_DIR}/Logs"
BUILD_LOG="${LOG_DIR}/ios-build.log"
XCODE_LOG="${LOG_DIR}/xcode-build.log"
BUILD_MODE="${1:-device}"
SIMULATOR_NAME="iPhone 16 Pro Max"
APP_BUNDLE_ID="com.seamuslawless.inkshot"
DEVELOPMENT_TEAM="28864BA964"
BUILD_START_TIME=""

if [[ "$BUILD_MODE" == "simulator" ]]; then
    XCODE_OUTPUT="${BUILD_DIR}/InkshotSimulator"
else
    XCODE_OUTPUT="${BUILD_DIR}/InkshotDev"
fi
XCODEPROJ="${XCODE_OUTPUT}/Unity-iPhone.xcodeproj"

# --- Colors ---
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
BOLD='\033[1m'
RESET='\033[0m'

# --- Helpers ---
info()    { echo -e "${CYAN}[INFO]${RESET}  $*"; }
success() { echo -e "${GREEN}[OK]${RESET}    $*"; }
warn()    { echo -e "${YELLOW}[WARN]${RESET}  $*"; }
fail()    { echo -e "${RED}[FAIL]${RESET}  $*"; }

print_duration() {
    local end_time
    end_time=$(date +%s)
    local elapsed=$(( end_time - BUILD_START_TIME ))
    local minutes=$(( elapsed / 60 ))
    local seconds=$(( elapsed % 60 ))
    info "Build duration: ${minutes}m ${seconds}s"
}

show_log_tail() {
    if [[ -f "$BUILD_LOG" ]]; then
        fail "Last 30 lines of ${BUILD_LOG}:"
        echo "---"
        tail -30 "$BUILD_LOG"
        echo "---"
    fi
}

cleanup() {
    local exit_code=$?
    if [[ $exit_code -ne 0 ]]; then
        fail "Build failed with exit code ${exit_code}"
        show_log_tail
        if [[ -n "$BUILD_START_TIME" ]]; then
            print_duration
        fi
    fi
}
trap cleanup EXIT

# --- Validation ---
usage() {
    echo -e "${BOLD}Usage:${RESET} ./build-ios.sh [device|simulator]"
    echo "  device     Build for physical iOS device (default)"
    echo "  simulator  Build for iOS Simulator and deploy"
    exit 1
}

if [[ "$BUILD_MODE" != "device" && "$BUILD_MODE" != "simulator" ]]; then
    fail "Unknown mode: ${BUILD_MODE}"
    usage
fi

if [[ ! -x "$UNITY_BIN" ]]; then
    fail "Unity binary not found at: ${UNITY_BIN}"
    exit 1
fi

if ! xcode-select -p &>/dev/null; then
    fail "Xcode command-line tools not found. Run: xcode-select --install"
    exit 1
fi

if ! command -v xcodebuild &>/dev/null; then
    fail "xcodebuild not found. Install Xcode from the App Store."
    exit 1
fi

# --- Setup ---
mkdir -p "$LOG_DIR" "$BUILD_DIR"

echo ""
echo -e "${BOLD}========================================${RESET}"
echo -e "${BOLD}  Inkshot iOS Build — ${BUILD_MODE} mode${RESET}"
echo -e "${BOLD}========================================${RESET}"
echo ""

BUILD_START_TIME=$(date +%s)

# --- Step 1: Unity batch build ---
info "Running Unity iOS build (logging to ${BUILD_LOG})..."

UNITY_ARGS=(
    -quit
    -batchmode
    -nographics
    -projectPath "$PROJECT_DIR"
    -executeMethod iOSBuilder.CommandLineBuild
    -logFile "$BUILD_LOG"
)

if [[ "$BUILD_MODE" == "simulator" ]]; then
    UNITY_ARGS+=(-simulatorBuild)
fi

"$UNITY_BIN" "${UNITY_ARGS[@]}"
success "Unity build completed"

# --- Step 2: Verify Xcode project ---
if [[ ! -d "$XCODEPROJ" ]]; then
    fail "Xcode project not found at: ${XCODEPROJ}"
    fail "Unity build may have failed silently. Check the log."
    show_log_tail
    exit 1
fi
success "Xcode project found: ${XCODEPROJ}"

# --- Step 3: Xcode build ---
if [[ "$BUILD_MODE" == "device" ]]; then
    info "Building for device (Debug)..."
    xcodebuild \
        -project "$XCODEPROJ" \
        -scheme Unity-iPhone \
        -configuration Debug \
        -destination 'generic/platform=iOS' \
        -allowProvisioningUpdates \
        DEVELOPMENT_TEAM="$DEVELOPMENT_TEAM" \
        2>&1 | tee "$XCODE_LOG" | tail -20

    success "Xcode device build completed (full log: ${XCODE_LOG})"
    print_duration
    echo ""
    echo -e "${GREEN}${BOLD}Build succeeded!${RESET}"
    echo ""
    info "Next steps:"
    echo "  1. Open ${XCODEPROJ} in Xcode"
    echo "     open \"${XCODEPROJ}\""
    echo "  2. Select your device in the toolbar"
    echo "  3. Press Run (Cmd+R)"
    echo ""

else
    info "Building for simulator..."
    DERIVED_DATA="${BUILD_DIR}/DerivedData"

    xcodebuild \
        -project "$XCODEPROJ" \
        -scheme Unity-iPhone \
        -configuration Debug \
        -destination "platform=iOS Simulator,name=${SIMULATOR_NAME}" \
        -derivedDataPath "$DERIVED_DATA" \
        DEVELOPMENT_TEAM="$DEVELOPMENT_TEAM" \
        2>&1 | tee "$XCODE_LOG" | tail -20

    success "Xcode simulator build completed (full log: ${XCODE_LOG})"

    # Find the .app bundle
    APP_PATH=$(find "$DERIVED_DATA" -name "*.app" -path "*/Debug-iphonesimulator/*" -type d | head -1)
    if [[ -z "$APP_PATH" ]]; then
        fail "Could not find .app bundle in DerivedData"
        exit 1
    fi
    success "App bundle: ${APP_PATH}"

    # Boot simulator if needed
    info "Booting simulator: ${SIMULATOR_NAME}..."
    xcrun simctl boot "${SIMULATOR_NAME}" 2>/dev/null || true
    open -a Simulator

    # Install and launch
    info "Installing app on simulator..."
    xcrun simctl install "${SIMULATOR_NAME}" "$APP_PATH"
    success "App installed"

    info "Launching app..."
    xcrun simctl launch "${SIMULATOR_NAME}" "$APP_BUNDLE_ID"
    success "App launched on ${SIMULATOR_NAME}"

    print_duration
    echo ""
    echo -e "${GREEN}${BOLD}Build succeeded! App is running in the simulator.${RESET}"
    echo ""
fi
