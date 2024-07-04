#!/bin/bash

# Define environment name and Python version
ENV_NAME="PerfectVision"
PYTHON_VERSION="3.11"
REQUIREMENTS_FILE="requirements.txt"

# Check if the requirements.txt file exists
if [ ! -f "$REQUIREMENTS_FILE" ]; then
  echo "Error: $REQUIREMENTS_FILE not found!"
  exit 1
fi

# Create the Conda environment
echo "Creating Conda environment '$ENV_NAME' with Python $PYTHON_VERSION..."
conda create --yes --prefix ./$ENV_NAME python=$PYTHON_VERSION

# Activate the environment
echo "Activating the environment..."
source activate ./$ENV_NAME

# Install the packages from requirements.txt
echo "Installing packages from $REQUIREMENTS_FILE..."
pip install -r $REQUIREMENTS_FILE

# Deactivate the environment
echo "Deactivating the environment..."
conda deactivate

# Print success message
echo "Environment '$ENV_NAME' created and packages installed successfully."
