-- Revo.Examples.Todos SQL baseline schema
-- MSSQL version

-- description: simple example app with tasks and task lists

CREATE TABLE TODOS_TODO_LIST_READ_MODEL (
	TODOS_TLI_TodoListReadModelId uniqueidentifier PRIMARY KEY,
	TODOS_TLI_Version int NOT NULL,
	TODOS_TLI_Name nvarchar(255)
);

CREATE TABLE TODOS_TODO_READ_MODEL (
	TODOS_TDO_TodoReadModelId uniqueidentifier PRIMARY KEY,
	TODOS_TDO_Version int NOT NULL,
	TODOS_TDO_TodoListId uniqueidentifier REFERENCES TODOS_TODO_LIST_READ_MODEL,
	TODOS_TDO_Text nvarchar(max),
	TODOS_TDO_IsComplete bit
);
