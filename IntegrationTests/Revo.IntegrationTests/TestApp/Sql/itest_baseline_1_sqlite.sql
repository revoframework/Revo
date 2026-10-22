-- Revo.IntegrationTests SQL baseline schema
-- SQLite version

-- description: test application for the integration tests

CREATE TABLE itest_test_list_read_model (
	itest_tli_test_list_read_model_id uuid PRIMARY KEY,
	itest_tli_version int NOT NULL,
	itest_tli_name text
);
