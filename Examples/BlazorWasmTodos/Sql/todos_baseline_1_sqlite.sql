-- Revo.Examples.Todos SQL baseline schema
-- SQLite version

-- description: simple example app with tasks and task lists

CREATE TABLE todos_todo_list_read_model (
	todos_tli_todo_list_read_model_id uuid PRIMARY KEY,
	todos_tli_version int NOT NULL,
	todos_tli_name text
);

CREATE TABLE todos_todo_read_model (
	todos_tdo_todo_read_model_id uuid PRIMARY KEY,
	todos_tdo_version int NOT NULL,
	todos_tdo_todo_list_id uuid REFERENCES todos_todo_list_read_model,
	todos_tdo_text text,
	todos_tdo_is_complete boolean
);
