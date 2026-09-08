set -e
# Print a pretty banner of the application starting
echo "Starting FEAR.Hosted .NET service..."

echo "=============================================================="
echo "   ______ ______    _    ____  "
echo "  |  ____|  ____|  / \  |  _ \ "
echo "  | |__  | |__    / _ \ | |_) |"
echo "  |  __| |  __|  / ___ \|    < "
echo "  | |    | |____/ /   \ \ |\  |"
echo "  |_|    |_____/_/     \_\| \_|"
echo ""
echo "   Forensic Extract and Representation (FEAR) Project"
echo "=============================================================="

# Waiting for the database to be ready before starting the application

# SystemTld from /run/secrets/SystemTld
SystemTld=$(cat /run/secrets/SystemTld)

# Set of domain names to check in host files is: api, ui, scripts
DnsNames=("api" "ui" "scripts")

# Check if the host files contain the expected domain names with the systemtld suffix
for dns in "${DnsNames[@]}"; do
    if ! grep -q "$dns.$SystemTld" /etc/hosts; then
        echo "[FEAR] Warning: Expected domain $dns.$SystemTld not found in /etc/hosts."
        echo "127.0.0.1 $dns.$SystemTld" >> /etc/hosts
    fi
done

echo "[FEAR] Waiting for database to be initialized..."
# Database readiness is guaranteed by Docker healthcheck on the postgres service
# (depends_on: condition: service_healthy), so no sleep is needed here.

echo "[FEAR] Initializing container environment..."
echo "$(cat /startup-marker.txt)"
echo "[FEAR] Loading configuration..."
echo "[FEAR] Preparing service startup sequence..."
export ASPNETCORE_ENVIRONMENT="Production"

echo "[FEAR] Starting service 1/2: Dotnet Web Application..."
echo "FEAR.Hosted .NET service"

cd /opt/fear/bin/fear-hosted
exec /usr/bin/dotnet FEAR.Hosted.dll &

echo "[FEAR] Dotnet Web Application started successfully."

echo "[FEAR] Starting service 2/2: Nginx Proxy..."
/usr/sbin/nginx -g 'daemon on; master_process on;' &


echo "[FEAR] Nginx Proxy started successfully."


echo "[FEAR] All FEAR services are online."

echo "[FEAR] System ready."

# date/time for creation
echo "application created at $(date)" > /.application-checkpoint

sleep infinity