ALTER TABLE rno_notification_buffer
	ADD COLUMN rno_nbf_name text NOT NULL DEFAULT '',
	ALTER COLUMN rno_nbf_pipeline_id TYPE text,
	ALTER COLUMN rno_nbf_governor_id TYPE text;

UPDATE rno_notification_buffer SET rno_nbf_name = rno_nbf_notification_buffer_id::text;

ALTER TABLE rno_notification_buffer
	ALTER COLUMN rno_nbf_name DROP DEFAULT;
ALTER TABLE rno_notification_buffer
	ADD UNIQUE(rno_nbf_name);

ALTER TABLE rno_notification_buffer
	RENAME COLUMN rno_nbf_pipeline_id TO rno_nbf_pipeline_name;

ALTER TABLE rno_notification_buffer
	RENAME COLUMN rno_nbf_governor_id TO rno_nbf_governor_name;