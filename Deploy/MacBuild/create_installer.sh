#!/bin/bash

# Define paths
PACKAGE_ROOT="./PackageContent"
APP_PATH="./PerfectVision.app"
PYTHON_PATH="./Python"
OUTPUT_PKG="./PerfectVision.pkg"
IDENTIFIER="com.jatin.perfectvision"
VERSION="1.0.5"
INSTALL_LOCATION="/Applications"  # Install location to /Applications

# Remove existing PerfectVision.pkg if it exists
if [ -f "$OUTPUT_PKG" ]; then
    echo "Removing existing $OUTPUT_PKG..."
    rm "$OUTPUT_PKG"
fi

# Remove existing PackageContent directory if it exists
if [ -d "$PACKAGE_ROOT" ]; then
    echo "Removing existing $PACKAGE_ROOT directory..."
    rm -rf "$PACKAGE_ROOT"
fi

# Recreate PackageContent directory structure
echo "Creating $PACKAGE_ROOT directory structure..."
mkdir -p "$PACKAGE_ROOT/Applications"
mkdir -p "$PACKAGE_ROOT/Library/Application Support/PerfectVision"

# Copy PerfectVision.app to PackageContent/Applications
echo "Copying PerfectVision.app to $PACKAGE_ROOT/Applications..."
cp -R "$APP_PATH" "$PACKAGE_ROOT/Applications/"

# Copy Python directory to PackageContent/Library/Application Support/PerfectVision
echo "Copying Python directory to $PACKAGE_ROOT/Library/Application Support/PerfectVision..."
cp -R "$PYTHON_PATH" "$PACKAGE_ROOT/Library/Application Support/PerfectVision/"

# Build the PKG installer
echo "Building the PKG installer..."
pkgbuild --root "$PACKAGE_ROOT" --identifier "$IDENTIFIER" --version "$VERSION" --install-location "$INSTALL_LOCATION" "$OUTPUT_PKG"

# Clear PackageContent directory
echo "Clearing temporal $PACKAGE_ROOT directory..."
rm -rf "$PACKAGE_ROOT"
echo "PKG installer created at $OUTPUT_PKG"