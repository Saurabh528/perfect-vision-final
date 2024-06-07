#!/bin/bash

# Define variables
APP_NAME="PerfectVision.app"
PYTHON_DIR="Python"
INSTALL_DIR="/Applications/PerfectVision"
PACKAGE_NAME="PerfectVision.pkg"
SRC_DIR="$(pwd)"
TEMP_DIR="temp_pkg"
COMPONENT_PKG="PerfectVisionComponent.pkg"

# Clean up any previous temporary directories
rm -rf "$TEMP_DIR"

# Create a temporary directory for packaging
echo "Creating temporary directory for packaging..."
mkdir -p "$TEMP_DIR/$INSTALL_DIR"

# Copy the application and Python directory to the temporary directory
echo "Copying application files to temporary directory..."
cp -R "$SRC_DIR/$APP_NAME" "$TEMP_DIR/$INSTALL_DIR/"
cp -R "$SRC_DIR/$PYTHON_DIR" "$TEMP_DIR/$INSTALL_DIR/"

# Verify the directory structure
echo "Verifying the directory structure..."
ls -lR "$TEMP_DIR"

# Build the component package
echo "Building the component package..."
pkgbuild --root "$TEMP_DIR" \
         --identifier "com.jatin.perfectvision" \
         --install-location "/" \
         --version "1.0" \
         "$COMPONENT_PKG"

# Create distribution XML
echo "Creating distribution XML..."
cat > distribution.xml <<EOF
<?xml version="1.0" encoding="utf-8"?>
<installer-gui-script minSpecVersion="1">
    <title>PerfectVision Installer</title>
    <background file="background.png" alignment="center" scaling="proportional"/>
    <welcome file="welcome.html"/>
    <license file="license.html"/>
    <choices-outline>
        <line choice="default">
            <line choice="perfectvision"/>
        </line>
    </choices-outline>
    <choice id="default"/>
    <choice id="perfectvision" title="PerfectVision" description="Install PerfectVision and Python" enabled="true" selected="true" start_selected="true">
        <pkg-ref id="com.jatin.perfectvision"/>
    </choice>
    <pkg-ref id="com.jatin.perfectvision" version="1.0" auth="Root">#${COMPONENT_PKG}</pkg-ref>
</installer-gui-script>
EOF

# Build the product package
echo "Building the product package..."
productbuild --distribution distribution.xml \
             --package-path . \
             --resources . \
             "$PACKAGE_NAME"

# Clean up temporary directory
echo "Cleaning up..."
rm -rf "$TEMP_DIR" "$COMPONENT_PKG" distribution.xml

echo "Package $PACKAGE_NAME created successfully."
