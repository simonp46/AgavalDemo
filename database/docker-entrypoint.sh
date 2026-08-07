#!/usr/bin/env bash

set -Eeuo pipefail

readonly SCRIPT_DIRECTORY="/docker-entrypoint-initdb.d"

if [[ -x /opt/mssql-tools18/bin/sqlcmd ]]; then
    readonly SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
elif [[ -x /opt/mssql-tools/bin/sqlcmd ]]; then
    readonly SQLCMD="/opt/mssql-tools/bin/sqlcmd"
else
    echo "sqlcmd is not available in the SQL Server image." >&2
    exit 1
fi

/opt/mssql/bin/sqlservr &
sqlserver_pid=$!

shutdown() {
    kill -TERM "${sqlserver_pid}" 2>/dev/null || true
    wait "${sqlserver_pid}" || true
}

trap shutdown SIGINT SIGTERM

echo "Waiting for SQL Server..."
until "${SQLCMD}" \
    -S localhost \
    -U sa \
    -P "${MSSQL_SA_PASSWORD}" \
    -C \
    -b \
    -Q "SELECT 1" > /dev/null 2>&1; do
    if ! kill -0 "${sqlserver_pid}" 2>/dev/null; then
        echo "SQL Server exited before becoming ready." >&2
        wait "${sqlserver_pid}"
        exit 1
    fi

    sleep 2
done

for script in "${SCRIPT_DIRECTORY}"/*.sql; do
    echo "Running ${script}..."
    "${SQLCMD}" \
        -S localhost \
        -U sa \
        -P "${MSSQL_SA_PASSWORD}" \
        -C \
        -b \
        -f 65001 \
        -i "${script}"
done

echo "GestorInventarioDB initialization completed."
wait "${sqlserver_pid}"
