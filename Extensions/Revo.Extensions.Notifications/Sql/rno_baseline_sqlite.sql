-- Revo.Extensions.Notifications SQL baseline schema for common providers (EF Core, EF6)
-- SQLite version
-- version: 2

-- NOTIFICATIONS

CREATE TABLE rno_notification_buffer (
	rno_nbf_notification_buffer_id uuid PRIMARY KEY,
	rno_nbf_name text NOT NULL UNIQUE,
	rno_nbf_pipeline_name text NOT NULL,
	rno_nbf_governor_name text NOT NULL
);

CREATE TABLE rno_buffered_notification (
	rno_bnt_buffered_notification_id uuid PRIMARY KEY,
	rno_bnt_notification_class_name text NOT NULL,
	rno_bnt_notification_json text NOT NULL,
	rno_bnt_buffer_id uuid NOT NULL REFERENCES rno_notification_buffer,
	rno_bnt_time_queued timestamptz NOT NULL
);

CREATE TABLE rno_apns_user_device_token (
	rno_aut_apns_user_device_token_id uuid PRIMARY KEY,
	rno_aut_version INT NOT NULL,
	rno_aut_user_id uuid NOT NULL,
	rno_aut_device_token text NOT NULL,
	rno_aut_app_id text NOT NULL,
	rno_aut_issued_date_time timestamptz NOT NULL
);

CREATE TABLE rno_apns_external_user_device_token (
	rno_aet_apns_external_user_device_token_id uuid PRIMARY KEY,
	rno_aet_version INT NOT NULL,
	rno_aet_external_user_id uuid NOT NULL,
	rno_aet_device_token text NOT NULL,
	rno_aet_app_id text NOT NULL,
	rno_aet_issued_date_time timestamptz NOT NULL
);

CREATE TABLE rno_fcm_user_device_token (
	rno_fut_fcm_user_device_token_id uuid PRIMARY KEY,
	rno_fut_version INT NOT NULL,
	rno_fut_user_id uuid NOT NULL,
	rno_fut_registration_id text NOT NULL,
	rno_fut_app_id text NOT NULL,
	rno_fut_issued_date_time timestamptz NOT NULL
);

CREATE TABLE rno_fcm_external_user_device_token (
	rno_fet_fcm_external_user_device_token_id uuid PRIMARY KEY,
	rno_fet_version INT NOT NULL,
	rno_fet_external_user_id uuid NOT NULL,
	rno_fet_registration_id text NOT NULL,
	rno_fet_app_id text NOT NULL,
	rno_fet_issued_date_time timestamptz NOT NULL
);
