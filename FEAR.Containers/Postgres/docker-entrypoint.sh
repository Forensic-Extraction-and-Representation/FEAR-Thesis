#!/bin/bash
set -e

# This will need to check for the presence of the /opt/deploy/deploy_config.json file
# If present, it will need to run the init python script to configure the environment

# Then it will need to boot the systems

if [ -f /.database-checkpoint ]; then
    echo "database already initialized at $(cat /.database-checkpoint)"
    su - postgres -c "pg_ctl -D /var/lib/postgresql/data -w start"
else
    su - postgres -c "initdb -D /var/lib/postgresql/data"
    sed -i "s/\#password_encryption.=..*\(\#.*\)/password_encryption = scram-sha-256 \1/" /var/lib/postgresql/data/postgresql.conf
    su - postgres -c "pg_ctl -D /var/lib/postgresql/data -w start"

    # path is /run/secrets/db_password for value
    export PGPWD="password" #"$(cat /run/secrets/db_password)"
    export USERNAME="fear_user"
    export PASSWORD="$PGPWD"

    # Run all setup SQL in a single session to ensure sequential execution
    su - postgres -c "psql" <<-EOSQL
        CREATE USER ${USERNAME} WITH PASSWORD '${PASSWORD}';
        CREATE DATABASE openid;
        GRANT ALL PRIVILEGES ON DATABASE openid TO ${USERNAME};
        CREATE DATABASE identity;
        GRANT ALL PRIVILEGES ON DATABASE identity TO ${USERNAME};
        CREATE DATABASE investigation;
        GRANT ALL PRIVILEGES ON DATABASE investigation TO ${USERNAME};
        CREATE DATABASE hosted_system;
        GRANT ALL PRIVILEGES ON DATABASE hosted_system TO ${USERNAME};
EOSQL

    sed -i "s/\#listen_addresses.=..*\(\#.*\)/listen_addresses = \'*\' \1/" /var/lib/postgresql/data/postgresql.conf
    echo -e "host\topenid\tfear_user\t0.0.0.0/0\tscram-sha-256" >> /var/lib/postgresql/data/pg_hba.conf
    echo -e "host\tidentity\tfear_user\t0.0.0.0/0\tscram-sha-256" >> /var/lib/postgresql/data/pg_hba.conf
    echo -e "host\tinvestigation\tfear_user\t0.0.0.0/0\tscram-sha-256" >> /var/lib/postgresql/data/pg_hba.conf
    echo -e "host\thosted_system\tfear_user\t0.0.0.0/0\tscram-sha-256" >> /var/lib/postgresql/data/pg_hba.conf

    su - postgres -c "pg_ctl -D /var/lib/postgresql/data -w restart"

    # Write checkpoints only after ALL setup is fully complete
    echo "database created at $(date)" > /.database-checkpoint
    echo "database ready at $(date)" > /.database-ready
fi

# On subsequent boots (checkpoint exists), re-write the ready marker if missing
if [ -f /.database-checkpoint ] && [ ! -f /.database-ready ]; then
    echo "database ready at $(date)" > /.database-ready
fi

sleep infinity