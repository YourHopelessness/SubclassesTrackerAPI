SELECT 'CREATE DATABASE ${DatabaseName} OWNER ${UserName};'
WHERE NOT EXISTS (SELECT 1 FROM pg_database WHERE datname = '${DatabaseName}');