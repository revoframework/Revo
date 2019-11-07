-- Revo.Extensions.History SQL baseline schema for common providers (EF Core, EF6)
-- PgSQL version

-- HISTORY

CREATE TABLE rhi_tracked_change_record (
	rhi_tch_tracked_change_record_id uuid PRIMARY KEY,
	rhi_tch_actor_name text NOT NULL,
	rhi_tch_user_id uuid,
	rhi_tch_aggregate_id uuid,
	rhi_tch_aggregate_class_id uuid,
	rhi_tch_entity_id uuid,
	rhi_tch_entity_class_id uuid,
	rhi_tch_change_time timestamptz NOT NULL,
	rhi_tch_change_data_json text NOT NULL,
	rhi_tch_change_data_class_name text NOT NULL
);

CREATE TABLE rhi_entity_attribute_data (
	rhi_ead_entity_attribute_data_id uuid PRIMARY KEY,
	rhi_ead_aggregate_id uuid,
	rhi_ead_entity_id uuid,
	rhi_ead_attribute_value_map_json text NOT NULL,
	rhi_ead_version int NOT NULL
);
