#!/bin/bash

# Check if required tools are installed
command -v openssl >/dev/null 2>&1 || { echo >&2 "OpenSSL is required but not installed. Aborting."; exit 1; }

# Configurable parameters
DAYS_VALID=365
KEY_BITS=2048
OUTPUT_DIR="./irc_certs"
SERVER_NAME="irc.example.com"
CONFIG_FILE="server.cnf"

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

# Verify configuration file exists
if [ ! -f "$CONFIG_FILE" ]; then
    echo "Error: Configuration file $CONFIG_FILE not found!"
    exit 1
fi

# Generate private key
echo "Generating private key..."
openssl genrsa -out "$OUTPUT_DIR/server.key" "$KEY_BITS"

# Generate certificate signing request
echo "Generating certificate signing request..."
openssl req -new -key "$OUTPUT_DIR/server.key" -out "$OUTPUT_DIR/server.csr" -config "$CONFIG_FILE"

# Generate self-signed certificate
echo "Generating self-signed certificate..."
openssl x509 -req -days "$DAYS_VALID" -in "$OUTPUT_DIR/server.csr" -signkey "$OUTPUT_DIR/server.key" -out "$OUTPUT_DIR/server.crt" -extensions req_ext -extfile "$CONFIG_FILE"

# Generate PFX file
echo "Generating PFX file..."
read -sp "Enter password for PFX file: " PFX_PASSWORD
echo ""

openssl pkcs12 -export -out "$OUTPUT_DIR/server.pfx" \
    -inkey "$OUTPUT_DIR/server.key" \
    -in "$OUTPUT_DIR/server.crt" \
    -passout pass:"$PFX_PASSWORD"

# Set correct permissions
chmod 600 "$OUTPUT_DIR/server.key"
chmod 644 "$OUTPUT_DIR/server.crt"
chmod 600 "$OUTPUT_DIR/server.pfx"

echo "Certificates generated successfully in $OUTPUT_DIR"
echo "- server.key: Private key"
echo "- server.csr: Certificate Signing Request"
echo "- server.crt: Certificate"
echo "- server.pfx: PKCS#12 certificate bundle"

# Clear password variable
unset PFX_PASSWORD
