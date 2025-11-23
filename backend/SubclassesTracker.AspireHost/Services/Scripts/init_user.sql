DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_roles WHERE rolname = '${UserName}') THEN
        EXECUTE format(
            'CREATE ROLE %I WITH LOGIN SUPERUSER PASSWORD %L',
            '${UserName}', '${UserPassword}'
        );
    END IF;
END $$;