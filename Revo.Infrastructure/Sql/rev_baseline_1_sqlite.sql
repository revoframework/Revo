-- Revo.Infrastructure SQL baseline schema for common providers (EF Core, EF6)
-- SQLite version

-- EVENT STORE

CREATE TABLE res_event_stream (
	res_evs_event_stream_id uuid NOT NULL PRIMARY KEY,
	res_evs_version int NOT NULL,
	res_evs_metadata_json text
);

CREATE TABLE res_event_stream_row (
	res_esr_event_stream_row_id uuid NOT NULL UNIQUE,
	res_esr_global_sequence_number INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
	res_esr_stream_id uuid NOT NULL REFERENCES res_event_stream,
	res_esr_stream_sequence_number bigint NOT NULL,
	res_esr_store_date timestamptz NOT NULL,
	res_esr_event_name text NOT NULL,
	res_esr_event_version int NOT NULL,
	res_esr_event_json text NOT NULL,
	res_esr_additional_metadata_json text,
	res_esr_is_dispatched_to_async_queues boolean NOT NULL,
	UNIQUE(res_esr_stream_id, res_esr_stream_sequence_number)
);

-- ASYNC EVENTS

CREATE TABLE rae_async_event_queue (
	rae_aeq_async_event_queue_id text NOT NULL PRIMARY KEY,
	rae_aeq_version int NOT NULL,
	rae_aeq_last_sequence_number_processed bigint
);

CREATE TABLE rae_external_event_record (
	rae_eer_external_event_record_id uuid NOT NULL PRIMARY KEY,
	rae_eer_version int NOT NULL,
	rae_eer_event_name text NOT NULL,
	rae_eer_event_version int NOT NULL,
	rae_eer_event_json text NOT NULL,
	rae_eer_metadata_json text,
	rae_eer_is_dispatched_to_async_queues boolean NOT NULL
);

CREATE TABLE rae_queued_async_event (
	rae_qae_queued_async_event_id uuid NOT NULL PRIMARY KEY,
	rae_qae_queue_id text NOT NULL REFERENCES rae_async_event_queue,
	rae_qae_sequence_number bigint,
	rae_qae_event_stream_row_id uuid REFERENCES res_event_stream_row(res_esr_event_stream_row_id),
	rae_qae_external_event_record_id uuid REFERENCES rae_external_event_record,
	UNIQUE(rae_qae_queue_id, rae_qae_sequence_number)
);

-- SAGAS

CREATE TABLE rev_saga_metadata_record (
	rev_smr_saga_metadata_record_id uuid NOT NULL PRIMARY KEY,
	rev_smr_class_id uuid NOT NULL
);

CREATE TABLE rev_saga_metadata_key (
	rev_smk_saga_metadata_key_id uuid NOT NULL PRIMARY KEY,
	rev_smk_saga_id uuid NOT NULL REFERENCES rev_saga_metadata_record,
	rev_smk_key_name text NOT NULL,
	rev_smk_key_value text NOT NULL
);
